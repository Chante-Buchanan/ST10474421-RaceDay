using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RaceDay.Api.Controllers;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTO;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ThenLogin_WithValidCredentials_Succeeds()
    {
        using var db = TestDatabase.CreateContext();

        var hasher = new PasswordHasher<User>();

        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "RaceDay_Test_Key_2026_1234567890_ABCDEF"
        };

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var controller = new AuthController(db, hasher, configuration);

        var registerDto = new RegisterDTO
        {
            FullName = "Pseudo Test Organiser",
            Email = "pseudo.organiser@test.local",
            Password = "TestPassword123!",
            Role = "Organiser",
            PhoneNumber = "0000000000"
        };

        var registerResult = await controller.Register(registerDto);

        Assert.IsType<OkObjectResult>(registerResult);

        var loginDto = new LoginDTO
        {
            Email = "pseudo.organiser@test.local",
            Password = "TestPassword123!"
        };

        var loginResult = await controller.Login(loginDto);

        var response = Assert.IsType<OkObjectResult>(loginResult);

        Assert.NotNull(response.Value);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var db = TestDatabase.CreateContext();

        var hasher = new PasswordHasher<User>();

        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "RaceDay_Test_Key_2026_1234567890_ABCDEF"
        };

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var user = new User
        {
            FullName = "Pseudo Test User",
            Email = "pseudo.user@test.local",
            Role = "Participant",
            PasswordHash = "",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = hasher.HashPassword(user, "CorrectPassword123!");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher, configuration);

        var dto = new LoginDTO
        {
            Email = "pseudo.user@test.local",
            Password = "WrongPassword123!"
        };

        var result = await controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }


    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        using var db = TestDatabase.CreateContext();

        var hasher = new PasswordHasher<User>();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        db.Users.Add(new User
        {
            FullName = "Existing Pseudo User",
            Email = "duplicate@test.local",
            PasswordHash = "test-only-hash",
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var controller = new AuthController(db, hasher, configuration);

        var dto = new RegisterDTO
        {
            FullName = "Second Pseudo User",
            Email = "duplicate@test.local",
            Password = "TestPassword123!",
            Role = "Participant"
        };

        var result = await controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Single(db.Users);
    }

}
