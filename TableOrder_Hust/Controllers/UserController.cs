using Microsoft.AspNetCore.Mvc;
using TableOrder_Hust.Models;
using TableOrder_Hust.Services;

namespace TableOrder_Hust.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Đăng ký tài khoản thường
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Password = request.Password
            };

            var createdUser = await _userService.RegisterAsync(request.Email, request.Password);
            return Ok(createdUser);
        }

        // Đăng nhập thường
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.LoginAsync(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(user);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var user = await _userService.GoogleLoginAsync(request.GoogleId, request.Email);
            return Ok(user);
        }
    }

    // DTO cho request
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class GoogleLoginRequest
    {
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
