using GuradzoApi.Models;
using GuradzoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GuradzoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserService _userService;

        public AuthController(TokenService tokenService, UserService userService)
        {
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (await _userService.ValidateUserAsync(request.UserName, request.Password))
            {
                var accessToken = _tokenService.GenerateAccessToken(request.UserName);
                var refreshToken = _tokenService.GenerateRefreshToken();
                await _tokenService.SaveRefreshTokenAsync(request.UserName, refreshToken);

                return Ok(new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });
            }

            return Unauthorized("Invalid credentials");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            if (await _userService.UserExistsAsync(request.UserName))
                return BadRequest("User already exists");

            try
            {
                await _userService.CreateUserAsync(request.UserName, request.Password);
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
                var newAccessToken = _tokenService.GenerateAccessToken(request.UserName);
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
    }
}
