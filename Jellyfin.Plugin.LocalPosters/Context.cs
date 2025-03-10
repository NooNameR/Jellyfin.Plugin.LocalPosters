using Jellyfin.Plugin.LocalPosters.Entities;
using MediaBrowser.Controller.Entities;
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
        var records = Set<PosterRecord>().Where(x => x.ImageType == ImageType.Primary).ToDictionary(x => x.ItemId);
        var count = manager.GetCount(new InternalItemsQuery());
        const int BatchSize = 5000;

        for (var startIndex = 0; startIndex < count; startIndex += BatchSize)
        {
            var hasChanges = false;
            foreach (var lItem in manager.GetItemList(new InternalItemsQuery
                     {
                         StartIndex = startIndex, Limit = BatchSize, SkipDeserialization = true
                     }))
            {
                if (!records.TryGetValue(lItem.Id, out var item))
                    continue;

                if (lItem.GetBaseItemKind() == item.ItemKind)
                    continue;

                item.ItemKind = lItem.GetBaseItemKind();
                item.ImageType = ImageType.Primary;
                hasChanges = true;
            }

            if (hasChanges)
                SaveChanges();
        }
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
