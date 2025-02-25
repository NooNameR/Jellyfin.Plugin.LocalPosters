using System.Drawing;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Utils;

public class BorderReplacerProvider(PluginConfiguration configuration)
{
    public IBorderReplacer Provide(BaseItemKind kind, ImageType imageType)
    {
        if (kind == BaseItemKind.Episode)
            return new SkiaDefaultBorderReplacer(new Size(1920, 1080));

        if (imageType != ImageType.Primary)
            throw new NotSupportedException($"Image type {imageType} is not supported");

        switch (kind)
        {
            case BaseItemKind.BoxSet:
            case BaseItemKind.Movie:
            case BaseItemKind.Series:
            case BaseItemKind.Season:
                return CreateBorderReplacer(new Size(1000, 1500));
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    IBorderReplacer CreateBorderReplacer(Size size)
    {
        if (!configuration.EnableBorderReplacer)
            return new SkiaDefaultBorderReplacer(size);

        if (configuration.RemoveBorder || !configuration.SkColor.HasValue)
            return new SkiaSharpBorderRemover(size);

        if (configuration.SkColor.HasValue)
            return new SkiaSharpBorderReplacer(size, configuration.SkColor.Value);

        return new SkiaDefaultBorderReplacer(size);
    }
}
