using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jellyfin.Plugin.LocalPosters.Entities;

/// <summary>
///
/// </summary>
public class PosterRecord
{
    private string _posterPath;

    private PosterRecord(Guid id, DateTimeOffset createdAt, string posterPath)
    {
        Id = id;
        CreatedAt = createdAt;
        _posterPath = posterPath;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="createdAt"></param>
    /// <param name="poster"></param>
    public PosterRecord(Guid id, DateTimeOffset createdAt, FileSystemMetadata poster) : this(id, createdAt, poster.FullName)
    {
    }

    /// <summary>
    /// File id
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///
    /// </summary>
    public DateTimeOffset MatchedAt { get; private set; }

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
        builder.HasKey(x => x.Id);
        builder.Property<string>("_posterPath").HasColumnName("PosterPath").IsRequired();
    }
}
