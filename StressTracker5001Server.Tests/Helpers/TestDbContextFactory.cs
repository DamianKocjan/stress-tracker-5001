using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Data;

namespace StressTracker5001Server.Tests.Helpers;

public static class TestDbContextFactory
{
  /// <summary>
  /// Creates an InMemory DbContext for testing purposes
  /// </summary>
  public static AppDbContext CreateInMemoryDbContext(string databaseName = "")
  {
    var dbName = string.IsNullOrEmpty(databaseName)
        ? Guid.NewGuid().ToString()
        : databaseName;

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: dbName)
        .Options;

    var context = new TestAppDbContext(options);
    context.Database.EnsureCreated();

    return context;
  }

  /// <summary>
  /// Test-specific DbContext that allows constructor injection
  /// </summary>
  private class TestAppDbContext : AppDbContext
  {
    private readonly DbContextOptions<AppDbContext> _options;

    public TestAppDbContext(DbContextOptions<AppDbContext> options)
    {
      _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {
        optionsBuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
      }
    }
  }

  /// <summary>
  /// Creates a DbContext with seeded test data
  /// </summary>
  public static AppDbContext CreateInMemoryDbContextWithData()
  {
    var context = CreateInMemoryDbContext();
    SeedTestData(context);
    return context;
  }

  private static void SeedTestData(AppDbContext context)
  {
    // Add common test data here if needed
    context.SaveChanges();
  }
}
