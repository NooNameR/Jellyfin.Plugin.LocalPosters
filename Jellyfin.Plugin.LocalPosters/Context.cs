using Jellyfin.Plugin.LocalPosters.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
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
    public const string DbName = "local-posters.db";

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

    //TODO: Remove this
    public void FixData(ILibraryManager manager)
    {
        var dbSet = Set<PosterRecord>();
        var records = dbSet.Where(x => x.ImageType == ImageType.Primary).ToArray();
        var hasChanges = false;
        foreach (var record in records)
        {
            var item = manager.GetItemById(record.ItemId);
            if (item == null)
                continue;

            record.ItemKind = item.GetBaseItemKind();
            dbSet.Update(record);
            hasChanges = true;
        }

        if (hasChanges)
            SaveChanges();
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
