using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RTMAuthServer.Models;
using RTMAuthServer.Data;
using Microsoft.AspNetCore.SignalR;
using RTMAuthServer.Hubs;
using System;
using System.Threading.Tasks;
using System.Linq;


namespace RTMAuthServer.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IHubContext<UserHub> _hubContext;
        public AuthController(AuthDbContext context, IHubContext<UserHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return BadRequest(new { success = false, message = "Username already exists" });

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Token = Guid.NewGuid().ToString(),
                LastLogin = null,
                LoginCount = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("UserUpdated", $"{username} registered.");
            return Ok(new { success = true, token = user.Token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return Unauthorized(new { success = false, message = "Invalid username or password" });

            if (string.IsNullOrEmpty(user.Token))
                user.Token = Guid.NewGuid().ToString();

            user.LoginCount++;
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UserUpdated", $"{username} logged in.");

            return Ok(new { success = true, token = user.Token });
        }

        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Token == token);
            if (user != null)
                return Ok(new { valid = true });
            return Ok(new { valid = false });
        }


        [HttpGet("admin/users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string token)
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Token == token && u.Role == "admin");
            if (admin == null)
                return Unauthorized("You are not authorized to view this data");

            var users = await _context.Users
                .Select(u => new { u.Username, u.Token, u.Role })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("admin/revoke")]
        public async Task<IActionResult> RevokeToken(string username, string token)
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Token == token && u.Role == "admin");
            if (admin == null)
                return Unauthorized("Not authorized");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return NotFound("User not found");

            user.Token = Guid.NewGuid().ToString() + "_revoked";
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UserUpdated", $"{username}'s token revoked.");

            return Ok(new { success = true, message = "Token revoked successfully" });
        }

    }
}
