using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTO;
using Xunit;

namespace RaceDay.Api.Tests;

public class ResultsControllerTests
{
    private static ResultsController CreateController(RaceDay.Api.Data.AppDbContext db,int userId)
    {
        var controller = new ResultsController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                            new Claim(ClaimTypes.Role, "Organiser")
                        },
                        "TestAuthentication"))
            }
        };

        return controller;
    }

    private static async Task<RaceDay.Api.Data.AppDbContext>
        CreateDatabaseWithEnrolment()
    {
        var db = TestDatabase.CreateContext();

        db.Events.Add(new Event
        {
            EventId = 1,
            OrganiserId = 10,
            EventName = "Pseudo Test Event",
            EventDate = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });

        db.Categories.Add(new Category
        {
            CategoryId = 1,
            EventId = 1,
            CategoryName = "Pseudo Test 5K",
            DistanceKm = 5,
            EntryFee = 50
        });

        db.Enrolments.Add(new Enrolment
        {
            EnrolmentId = 1,
            CategoryId = 1,
            ParticipantId = 20,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed",
            PaymentStatus = "Paid"
        });

        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task CreateResult_ValidData_ReturnsOk()
    {
        using var db = await CreateDatabaseWithEnrolment();
        var controller = CreateController(db, 10);

        var dto = new ResultDTO
        {
            EnrolmentId = 1,
            FinishTime = TimeSpan.FromMinutes(30),
            Position = 1,
            Status = "Finished"
        };

        var response = await controller.CreateResult(dto);

        Assert.IsType<OkObjectResult>(response);
        Assert.Equal(1, await db.Results.CountAsync());

        var result = await db.Results.FirstAsync();
        Assert.Equal(1, result.EnrolmentId);
        Assert.Equal("Finished", result.Status);
        Assert.Equal(1, result.Position);
    }

    [Fact]
    public async Task CreateResult_InvalidStatus_ReturnsBadRequest()
    {
        using var db = await CreateDatabaseWithEnrolment();
        var controller = CreateController(db, 10);

        var dto = new ResultDTO
        {
            EnrolmentId = 1,
            FinishTime = TimeSpan.FromMinutes(30),
            Position = 1,
            Status = "Invalid"
        };

        var response = await controller.CreateResult(dto);

        Assert.IsType<BadRequestObjectResult>(response);
        Assert.Empty(await db.Results.ToListAsync());
    }
}
