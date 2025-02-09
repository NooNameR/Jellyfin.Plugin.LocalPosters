﻿using Jellyfin.Plugin.LocalPosters.Utils;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SkiaSharpBorderReplacerTests
{
    private readonly FileInfo _source;

    public SkiaSharpBorderReplacerTests()
    {
        _source = new FileInfo("");
    }

    // [Fact]
    public void TestBorderRemover()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_removed" + _source.Extension);
        var borderReplacer = new SkiaSharpBorderReplacer();

        borderReplacer.RemoveBorder(_source.FullName, target.FullName);
    }

    // [Fact]
    public void TestBorderReplacer()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_replaced" + _source.Extension);
        var borderReplacer = new SkiaSharpBorderReplacer();

        borderReplacer.ReplaceBorder(_source.FullName, target.FullName, SKColors.SkyBlue);
    }
}
