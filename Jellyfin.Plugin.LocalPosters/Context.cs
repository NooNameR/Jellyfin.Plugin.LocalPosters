using Jellyfin.Plugin.LocalPosters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jellyfin.Plugin.LocalPosters;

/// <summary>
///
/// </summary>
public class Context : DbContext
{
    /// <summary>
    ///
    /// </summary>
    public static readonly string DbName = "local-posters.db";

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        new PosterRecordConfiguration().Configure(modelBuilder.Entity<PosterRecord>());
    }

    /// <summary>
    ///
    /// </summary>
    public void ApplyMigration()
    {
        // If database doesn't exist or can't connect, create it with migrations
        if (!Database.CanConnect())
        {
            Database.Migrate();
            return;
        }

        // If migrations table exists, apply pending migrations normally
        if (Database.GetAppliedMigrations().Any()) Database.Migrate();
    }
}

/// <summary>
///
/// </summary>
public class ContextFactory : IDesignTimeDbContextFactory<Context>
{
    /// <inheritdoc />
    public Context CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Context>();
        optionsBuilder.UseSqlite($"Data Source={Context.DbName}")
            .EnableSensitiveDataLogging(false);

        return new Context(optionsBuilder.Options);
    }
}
