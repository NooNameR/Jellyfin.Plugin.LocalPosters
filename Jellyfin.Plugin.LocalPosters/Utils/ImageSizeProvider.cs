using System.Drawing;
using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Utils;

public class ImageSizeProvider
{
    public Size GetImageSize(BaseItemKind kind, ImageType imageType)
    {
        return imageType switch
        {
            ImageType.Primary => kind switch
            {
                BaseItemKind.BoxSet or BaseItemKind.Movie or BaseItemKind.Series or BaseItemKind.Season => new Size(1000, 1500),
                BaseItemKind.Episode => new Size(1920, 1080),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            },
            ImageType.Backdrop => new Size(1920, 1080),
            ImageType.Art => new Size(500, 281),
            ImageType.Banner => new Size(1000, 185),
            ImageType.Thumb => new Size(1000, 562),
            ImageType.Logo => new Size(800, 310),
            ImageType.Disc => new Size(1000, 1000),
            _ => throw new NotSupportedException("Not supported image type")
        };
    }
}
