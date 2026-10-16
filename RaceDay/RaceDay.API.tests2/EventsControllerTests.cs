using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTO;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventsControllerTests
{
    [Fact]
    public async Task CreateEvent_WithValidData_ReturnsCreatedResult()
    {
        using var db = TestDatabase.CreateContext();

        db.Users.Add(new User
        {
            UserId = 10,
            FullName = "Test Organiser",
            Email = "organiser@test.local",
            PasswordHash = "fake-test-hash",
            Role = "Organiser"
        });

        await db.SaveChangesAsync();

        var controller = new EventsController(db);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10")
        }, "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var dto = new EventDTO
        {
            EventName = "Test Race",
            Description = "Test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Park",
            City = "Test City",
            Province = "Test Province"
        };

        var result = await controller.CreateEvent(dto);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Events);
    }
}
