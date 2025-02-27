using System.Drawing;
using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Utils;

public class ImageSizeProvider
{
    public Size GetImageSize(BaseItemKind kind, ImageType imageType)
    {
        if (imageType != ImageType.Primary)
            throw new NotSupportedException($"Image type {imageType} is not supported");

        switch (kind)
        {
            case BaseItemKind.BoxSet:
            case BaseItemKind.Movie:
            case BaseItemKind.Series:
            case BaseItemKind.Season:
                return new Size(1000, 1500);
            case BaseItemKind.Episode:
                return new Size(1920, 1080);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }
}
