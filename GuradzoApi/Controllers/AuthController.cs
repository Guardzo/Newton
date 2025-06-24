using GuradzoApi.Interfaces;
using GuradzoApi.Models;
using GuradzoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuradzoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AuthController(ITokenService tokenService, IUserService userService)
        {
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (await _userService.ValidateUserAsync(request.UserName, request.Password))
            {
                var user = await _userService.GetUserAsync(request.UserName);
                if (user == null) return Unauthorized();

                var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                await _tokenService.SaveRefreshTokenAsync(user.Username, refreshToken);

                return Ok(new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });
            }

            return Unauthorized("Invalid credentials");
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _userService.UserExistsAsync(request.Username))
                return BadRequest("User already exists");

            try
            {
                await _userService.CreateUserAsync(request.Username, request.Password, request.Role);
                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] LoginRequest request)
        {
            if (await _tokenService.ValidateRefreshTokenAsync(request.UserName, request.Password))
            {
                var user = await _userService.GetUserAsync(request.UserName);
                if (user == null) return Unauthorized();

                var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                await _tokenService.SaveRefreshTokenAsync(request.UserName, newRefreshToken);

                return Ok(new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }

            return Unauthorized("Invalid refresh token");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _userService.ResetPasswordAsync(request.Username, request.NewPassword);
            return result
                ? Ok("Password updated successfully")
                : NotFound("User not found");
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            return Ok($"Hello {username}, you are authenticated.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-data")]
        public IActionResult GetAdminData()
        {
            return Ok("This is only accessible to Admins");
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-data")]
        public IActionResult GetUserData()
        {
            return Ok("This is only accessible to Users");
        }
    }
}
