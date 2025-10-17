using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Neko_api.Data;
using Neko_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Neko_api.Middleware
{
    public class JwtLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public JwtLoggingMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Log request
            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

            // Example: auto-generate JWT if header contains "X-Generate-JWT: userId"
            if (context.Request.Headers.TryGetValue("X-Generate-JWT", out var userIdHeader))
            {
                if (int.TryParse(userIdHeader, out int userId))
                {
                    var user = await dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        // Fetch role
                        var userRole = await dbContext.User_roles.FirstOrDefaultAsync(r => r.userid == user.Userid);
                        string roleName = "User";
                        if (userRole != null)
                        {
                            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.roleid == userRole.roleid);
                            if (role != null) roleName = role.rolename;
                        }

                        // Generate JWT
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

                        context.Response.Headers.Add("X-JWT-Token", tokenString);
                        Console.WriteLine($"Generated JWT for user {user.Username}: {tokenString}");
                    }
                }
            }

            // Call the next middleware
            await _next(context);

            // Log response status
            Console.WriteLine($"Response: {context.Response.StatusCode}");
        }
    }
}
