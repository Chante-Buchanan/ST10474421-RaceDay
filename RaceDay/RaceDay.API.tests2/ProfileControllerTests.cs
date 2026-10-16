using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Models;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Api.Tests;

public class ProfileControllerTests
{
    [Fact]
    public async Task GetProfile_ExistingUser_ReturnsOk()
    {
        // Arrange
        using var db = TestDatabase.CreateContext();

        var user = new User
        {
            FullName = "Pseudo Test User",
            Email = "pseudo.profile@test.local",
            PasswordHash = "test-only-hash",
            PhoneNumber = "0000000000",
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new ProfileController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };

        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString())
                },
                "TestAuthentication"));

        // Act
        var result = await controller.GetProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetProfile_NonexistentUser_ReturnsNotFound()
    {
        // Arrange
        using var db = TestDatabase.CreateContext();

        var controller = new ProfileController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };

        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "999")
                },
                "TestAuthentication"));

        // Act
        var result = await controller.GetProfile();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found.", notFoundResult.Value);
    }
}
