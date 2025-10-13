using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neko_api.Data;
using Neko_api.Models;

namespace Neko_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("signup")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            bool exists = await _context.Users.AnyAsync(u =>
                u.Username == user.Username || u.Email == user.Email || u.Mobile == user.Mobile);

            if (exists)
                return BadRequest("A user with the same Username, Email, or Mobile already exists.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.Userid}, user);
        }

        [HttpGet("checkuser")]
        public async Task<IActionResult> CheckUser(string username, string email, string mobile)
        {
            // Check each field in the database
            var usernameExists = await _context.Users.AnyAsync(u => u.Username == username);
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            var mobileExists = await _context.Users.AnyAsync(u => u.Mobile == mobile);

            // Return detailed response
            return Ok(new
            {
                usernameExists,
                emailExists,
                mobileExists
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User_login request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.EmailOrMobile || u.Mobile == request.EmailOrMobile);

            if (user == null)
                return Unauthorized("Invalid credentials. Please try again.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isPasswordValid)
                return Unauthorized("Invalid credentials. Please try again.");

            return Ok(new { message = "Login successful", user });
        }
    }
}
