using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ResourceManagementSystem.API.Controllers;
using ResourceManagementSystem.API.Data;
using ResourceManagementSystem.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ResourceManagementSystem.Tests
{
    public class AuthControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            return new ApplicationDbContext(options);
        }

        private IConfiguration GetMockConfiguration()
        {
            var settings = new Dictionary<string, string>
            {
                {"Jwt:Key", "super_secret_test_key_123456789"},
                {"Jwt:Issuer", "test_issuer"},
                {"Jwt:Audience", "test_audience"},
                {"Jwt:ExpiresInMinutes", "60"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenNewUserIsRegistered()
        {
            // Arrange
            var dbContext = GetDbContext();
            var config = GetMockConfiguration();
            var controller = new AuthController(dbContext, config);

            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Registration successful", okResult.Value);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            var dbContext = GetDbContext();
            var config = GetMockConfiguration();
            var controller = new AuthController(dbContext, config);

            dbContext.Users.Add(new User { Email = "existing@example.com" });
            dbContext.SaveChanges();

            var request = new RegisterRequest
            {
                Username = "user2",
                Email = "existing@example.com",
                Password = "AnotherPassword"
            };

            var result = await controller.Register(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            var dbContext = GetDbContext();
            var config = GetMockConfiguration();
            var controller = new AuthController(dbContext, config);

            var result = await controller.Login(new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            var dbContext = GetDbContext();
            var config = GetMockConfiguration();
            var controller = new AuthController(dbContext, config);

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("ValidPassword");
            dbContext.Users.Add(new User
            {
                Username = "user",
                Email = "valid@example.com",
                Role = "User",
                PasswordHash = hashedPassword
            });
            dbContext.SaveChanges();

            var result = await controller.Login(new LoginRequest
            {
                Email = "valid@example.com",
                Password = "ValidPassword"
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}
