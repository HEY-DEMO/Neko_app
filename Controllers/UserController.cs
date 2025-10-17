using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Neko_api.Data;
using Neko_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Neko_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("signup")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            bool exists = await _context.Users.AnyAsync(u =>
                u.Username == user.Username || u.Email == user.Email || u.Mobile == user.Mobile);

            if (exists)
                return BadRequest("A user with the same Username, Email, or Mobile already exists.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.rolename == "User");
                if (userRole != null)
                {
                    _context.User_roles.Add(new User_roles { userid = user.Userid, roleid = userRole.roleid });
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return CreatedAtAction(nameof(GetUsers), new { id = user.Userid }, user);
        }

        [HttpGet("checkuser")]
        public async Task<IActionResult> CheckUser(string username, string email, string mobile)
        {
            return Ok(new
            {
                usernameExists = await _context.Users.AnyAsync(u => u.Username == username),
                emailExists = await _context.Users.AnyAsync(u => u.Email == email),
                mobileExists = await _context.Users.AnyAsync(u => u.Mobile == mobile)
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User_login request)
        {
            // 1️⃣ Check if user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.EmailOrMobile || u.Mobile == request.EmailOrMobile);

            if (user == null)
                return NotFound(new { message = "No account found with this email or mobile." });

            // 2️⃣ Check password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized(new { message = "Invalid credentials." });

            // 3️⃣ Get user role
            var userRole = await _context.User_roles.FirstOrDefaultAsync(r => r.userid == user.Userid);
            string roleName = "User";
            if (userRole != null)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleid == userRole.roleid);
                if (role != null) roleName = role.rolename;
            }

            // 4️⃣ Generate JWT token
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Userid.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", roleName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 5️⃣ Revoke previous refresh tokens
            var existingTokens = _context.RefreshTokens.Where(rt => rt.Userid == user.Userid && !rt.IsRevoked);
            foreach (var t in existingTokens) t.IsRevoked = true;

            var refreshToken = new RefreshToken
            {
                Userid = user.Userid,
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Login successful",
                token = tokenString,
                expires = token.ValidTo,
                refreshToken = refreshToken.Token,
                user = new { user.Userid, user.Username, user.Email, Role = roleName }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var tokenEntry = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (tokenEntry == null || tokenEntry.Expires < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == tokenEntry.Userid);
            if (user == null) return Unauthorized();

            var userRole = await _context.User_roles.FirstOrDefaultAsync(r => r.userid == user.Userid);
            string roleName = "User";
            if (userRole != null)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.roleid == userRole.roleid);
                if (role != null) roleName = role.rolename;
            }

            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Userid.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", roleName)
            };

            var newToken = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );

            tokenEntry.IsRevoked = true; // Revoke old refresh token
            var newRefreshToken = new RefreshToken
            {
                Userid = user.Userid,
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newToken),
                expires = newToken.ValidTo,
                refreshToken = newRefreshToken.Token
            });
        }
    }
}
