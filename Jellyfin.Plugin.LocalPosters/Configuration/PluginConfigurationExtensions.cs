using MediaBrowser.Model.IO;

namespace Jellyfin.Plugin.LocalPosters.Configuration;

public static class PluginConfigurationExtensions
{
    static readonly Lazy<FileSystemMetadata> _empty = new(() => new FileSystemMetadata { Exists = false });

    public static FileSystemMetadata GoogleSaCredentialFile(this PluginConfiguration configuration, IFileSystem fileSystem)
    {
        return string.IsNullOrEmpty(configuration.GoogleSaCredentialFile)
            ? _empty.Value
            : fileSystem.GetFileInfo(configuration.GoogleSaCredentialFile);
    }

    public static FileSystemMetadata GoogleClientSecretFile(this PluginConfiguration configuration, IFileSystem fileSystem)
    {
        return string.IsNullOrEmpty(configuration.GoogleClientSecretFile)
            ? _empty.Value
            : fileSystem.GetFileInfo(configuration.GoogleClientSecretFile);
    }
}
