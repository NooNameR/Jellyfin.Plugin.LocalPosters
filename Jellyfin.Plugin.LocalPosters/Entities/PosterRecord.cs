using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Jellyfin.Plugin.LocalPosters.Entities;

/// <summary>
///
/// </summary>
public class PosterRecord
{
    private string _posterPath;

    private PosterRecord(Guid itemId, BaseItemKind itemKind, ImageType imageType, DateTimeOffset createdAt, string posterPath)
    {
        ItemId = itemId;
        ItemKind = itemKind;
        ImageType = imageType;
        CreatedAt = createdAt;
        _posterPath = posterPath;
    }

    public PosterRecord(BaseItem item, ImageType imageType, DateTimeOffset createdAt, FileSystemMetadata poster) : this(item.Id,
        item.GetBaseItemKind(), imageType, createdAt, poster.FullName)
    {
    }

    /// <summary>
    /// File id
    /// </summary>
    public Guid ItemId { get; private init; }

    /// <summary>
    ///
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }

    /// <summary>
    ///
    /// </summary>
    public DateTimeOffset MatchedAt { get; private set; }

    // TODO: replace after migration
    /// <summary>
    ///
    /// </summary>
    public ImageType ImageType { get; set; }

    // TODO: replace after migration
    /// <summary>
    ///
    /// </summary>
    public BaseItemKind ItemKind { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <returns></returns>
    public FileSystemMetadata PosterFile(IFileSystem fileSystem)
    {
        return fileSystem.GetFileInfo(_posterPath);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="now"></param>
    public void SetPosterFile(FileSystemMetadata path, DateTimeOffset now)
    {
        if (!path.Exists)
            throw new FileNotFoundException("File does not exist", path.FullName);

        MatchedAt = now;
        _posterPath = path.FullName;
    }
}

/// <summary>
///
/// </summary>
public class PosterRecordConfiguration : IEntityTypeConfiguration<PosterRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PosterRecord> builder)
    {
        builder.HasKey(x => new { x.ItemId, x.ImageType });
        builder.Property<string>("_posterPath")
            .HasColumnName("PosterPath")
            .IsRequired();

        builder.Property(t => t.ItemKind)
            .HasConversion(new EnumToStringConverter<BaseItemKind>())
            .IsRequired();
        builder.Property(t => t.ImageType)
            .HasConversion(new EnumToStringConverter<ImageType>())
            .HasDefaultValue(ImageType.Primary)
            .IsRequired();

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}
