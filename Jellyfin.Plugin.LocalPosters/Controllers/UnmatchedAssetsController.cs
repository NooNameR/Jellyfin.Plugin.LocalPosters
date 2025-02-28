using System.Net.Mime;
using J2N.Collections.ObjectModel;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.LocalPosters.Controllers;

[Authorize]
[ApiController]
[Route("LocalPosters/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class UnmatchedAssetsController(
    IQueryable<PosterRecord> queryable,
    IMatcherFactory matcherFactory,
    ILibraryManager libraryManager,
    IProviderManager providerManager,
    IDirectoryService directoryService,
    IDtoService dtoService) : ControllerBase
{
    const int BatchSize = 5000;

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet("{kind}/{type}")]
    public async Task<ActionResult<IEnumerable<BaseItemDto>>> Get([FromRoute] BaseItemKind kind, [FromRoute] ImageType type)
    {
        if (type != ImageType.Primary)
            return BadRequest("Image type must be Primary");
        if (!matcherFactory.SupportedItemKinds.Contains(kind))
            return BadRequest("Invalid item kind");

        var dict = new Dictionary<Guid, BaseItem>();
        var records = libraryManager.GetCount(new InternalItemsQuery { IncludeItemTypes = [kind], ImageTypes = [type] });
        var ids = new HashSet<Guid>(await queryable.Select(x => x.Id).ToListAsync(HttpContext.RequestAborted).ConfigureAwait(false));
        var imageRefreshOptions = new ImageRefreshOptions(directoryService)
        {
            ImageRefreshMode = MetadataRefreshMode.FullRefresh, ReplaceImages = [type]
        };

        for (var startIndex = 0; startIndex < records; startIndex += BatchSize)
        {
            foreach (var item in libraryManager.GetItemList(new InternalItemsQuery { IncludeItemTypes = [kind], ImageTypes = [type] }))
            {
                HttpContext.RequestAborted.ThrowIfCancellationRequested();

                if (!ids.Contains(item.Id) && providerManager.HasImageProviderEnabled(item, imageRefreshOptions))
                    dict.Add(item.Id, item);
            }
        }

        var dtoOptions = new DtoOptions { Fields = [ItemFields.Path], ImageTypes = [type], EnableImages = true };

        return Ok(dtoService.GetBaseItemDtos(new ReadOnlyList<BaseItem>(dict.Values.ToList()), dtoOptions));
    }
}
