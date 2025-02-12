using System.Diagnostics.CodeAnalysis;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jellyfin.Plugin.LocalPosters.Entities;

/// <summary>
///
/// </summary>
public class PosterRecord
{
    private static readonly Lazy<FileSystemMetadata> _emptyFile = new(() => new FileSystemMetadata { Exists = false });
    private string? _posterPath;

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="createdAt"></param>
    public PosterRecord(Guid id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
        _posterPath = null;
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
    /// <param name="fileSystem"></param>
    /// <returns></returns>
    public FileSystemMetadata PosterFile(IFileSystem fileSystem)
    {
        return !HasPoster ? _emptyFile.Value : fileSystem.GetFileInfo(_posterPath);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    public void SetPosterFile(FileSystemMetadata path)
    {
        _posterPath = path.FullName;
    }

    /// <summary>
    ///
    /// </summary>
    [MemberNotNullWhen(true, nameof(_posterPath))]
    public bool HasPoster => !string.IsNullOrEmpty(_posterPath);
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
        builder.Property<string?>("_posterPath").IsRequired(false);
    }
}
