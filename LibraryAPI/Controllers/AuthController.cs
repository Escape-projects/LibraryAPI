using LibraryAPI.Contracts.DTOs.Auth;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService AuthService)
        {
            authService = AuthService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            await authService.RegisterAsync(registerDto);
            return StatusCode(201, new { message = "Registration success" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var tokens = await authService.LoginAsync(loginDTO);
            return Ok(tokens);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenDTO token)
        {
            var tokens = await authService.RefreshAsync(token);
            return Ok(tokens);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenDTO token)
        {
            await authService.LogoutAsync(token);
            return NoContent();
        }
    }
}
