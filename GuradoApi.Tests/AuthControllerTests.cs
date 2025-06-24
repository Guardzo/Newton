using GuradzoApi.Controllers;
using GuradzoApi.Data;
using GuradzoApi.Interfaces;
using GuradzoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuradoApi.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_ValidUser_ReturnsTokenResponse()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockTokenService = new Mock<ITokenService>();

            var request = new LoginRequest { UserName = "admin", Password = "pass" };
            var user = new User { Username = "admin", Role = "Admin" };

            mockUserService.Setup(s => s.ValidateUserAsync(request.UserName, request.Password)).ReturnsAsync(true);
            mockUserService.Setup(s => s.GetUserAsync(request.UserName)).ReturnsAsync(user);
            mockTokenService.Setup(s => s.GenerateAccessTokenAsync(user)).ReturnsAsync("access-token");
            mockTokenService.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");

            var controller = new AuthController(mockTokenService.Object, mockUserService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("access-token", response.AccessToken);
            Assert.Equal("refresh-token", response.RefreshToken);
        }

        [Fact]
        public async Task Login_InvalidUser_ReturnsUnauthorized()
        {
            var mockUserService = new Mock<IUserService>();
            var mockTokenService = new Mock<ITokenService>();
            var controller = new AuthController(mockTokenService.Object, mockUserService.Object);

            var request = new LoginRequest { UserName = "baduser", Password = "wrongpass" };

            mockUserService.Setup(s => s.ValidateUserAsync(request.UserName, request.Password)).ReturnsAsync(false);

            var result = await controller.Login(request);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials", unauthorized.Value);
        }
    }
}
