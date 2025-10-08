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

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            // Check if Name, Email, or Mobile already exists
            bool exists = await _context.Users.AnyAsync(u =>
                u.Name == user.Name || u.Email == user.Email || u.Mobile == user.Mobile);

            if (exists)
            {
                return BadRequest("A user with the same Name, Email, or Mobile already exists.");
            }

            // Insert new user
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

    }
}
