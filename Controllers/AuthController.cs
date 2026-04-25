using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_Cafeteria.Models;

namespace POS_Cafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PosContext _context;

        public AuthController(PosContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // En un sistema real usarías hash para la contraseña, 
            // aquí comparamos directo para fines educativos.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

            return Ok(new { 
                userId = user.UserId, 
                username = user.Username,
                roleId = 1 // Podrías traer el rol real de la tabla ROLE
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}