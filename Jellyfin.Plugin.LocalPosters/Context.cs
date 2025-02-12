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
    /// <param name="options"></param>
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    /// <summary>
    ///
    /// </summary>
    public static readonly string ConnectionString = "Data Source=local-posters.db";

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        new PosterRecordConfiguration().Configure(modelBuilder.Entity<PosterRecord>());
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
        optionsBuilder.UseSqlite(Context.ConnectionString)
            .EnableSensitiveDataLogging(false);

        return new Context(optionsBuilder.Options);
    }
}
