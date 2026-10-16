using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public static class TestDatabase
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: $"RaceDayTest_{Guid.NewGuid()}").Options;

        return new AppDbContext(options);
    }
}
