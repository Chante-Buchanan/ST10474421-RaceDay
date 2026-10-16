using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Models;
using RaceDay.Api.Models.DTO;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrolmentsControllerTests
{
    [Fact]
    public async Task CreateEnrolment_ValidCategory_ReturnsOk()
    {
        using var db = TestDatabase.CreateContext();

        db.Categories.Add(new Category
        {
            EventId = 1,
            CategoryName = "Pseudo Test 5K",
            DistanceKm = 5,
            EntryFee = 50
        });

        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "10")
                },
                "Test"));

        var result = await controller.CreateEnrolment(new EnrolmentDTO { CategoryId = 1 });

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(db.Enrolments);
    }


    [Fact]
    public async Task CreateEnrolment_MissingCategory_ReturnsNotFound()
    {
        using var db = TestDatabase.CreateContext();

        var controller = new EnrolmentsController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, "10")
                        },
                        "Test"))
            }
        };

        var result = await controller.CreateEnrolment(new EnrolmentDTO { CategoryId = 999 });

        Assert.IsType<NotFoundObjectResult>(result);
    }



}

