using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetCategories_ReturnsCategories()
    {
        // Arrange: use an isolated in-memory database.
        using var db = TestDatabase.CreateContext();

        db.Categories.Add(new Category
        {
            EventId = 1,
            CategoryName = "5K",
            DistanceKm = 5,
            EntryFee = 50
        });

        await db.SaveChangesAsync();

        var controller = new CategoriesController(db);

        // Act
        var result = await controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);

        Assert.Single(categories);
        Assert.Equal("5K", categories.First().CategoryName);
    }

    [Fact]
    public async Task GetEventCategories_EventDoesNotExist_ReturnsNotFound()
    {
        // Arrange: the database contains no events.
        using var db = TestDatabase.CreateContext();

        var controller = new CategoriesController(db);

        // Act
        var result = await controller.GetEventCategories(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}

