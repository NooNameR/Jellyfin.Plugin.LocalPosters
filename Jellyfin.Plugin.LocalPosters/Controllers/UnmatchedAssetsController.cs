using System.Net.Mime;
using J2N.Collections.ObjectModel;
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
[Route("LocalPosters/[controller]/[action]")]
[Produces(MediaTypeNames.Application.Json)]
public class UnmatchedAssetsController(
    IQueryable<PosterRecord> queryable,
    IMatcherFactory matcherFactory,
    ILibraryManager libraryManager,
    IProviderManager providerManager,
    IDirectoryService directoryService,
    IDtoService dtoService) : ControllerBase
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaseItemDto>>> Get()
    {
        var dict = new Dictionary<Guid, BaseItem>();

        var ids = new HashSet<Guid>(await queryable.Select(x => x.Id).ToListAsync(HttpContext.RequestAborted).ConfigureAwait(false));
        var imageRefreshOptions = new ImageRefreshOptions(directoryService)
        {
            ImageRefreshMode = MetadataRefreshMode.FullRefresh, ReplaceImages = [ImageType.Primary]
        };

        foreach (var item in libraryManager.GetItemList(new InternalItemsQuery
                 {
                     IncludeItemTypes = [..matcherFactory.SupportedItemKinds], ExcludeItemIds = [..ids]
                 }))
        {
            HttpContext.RequestAborted.ThrowIfCancellationRequested();

            if (providerManager.HasImageProviderEnabled(item, imageRefreshOptions))
                dict.Add(item.Id, item);
        }

        var dtoOptions = new DtoOptions { Fields = [ItemFields.Path], ImageTypes = [ImageType.Primary], EnableImages = true };

        return Ok(dtoService.GetBaseItemDtos(new ReadOnlyList<BaseItem>(dict.Values.ToList()), dtoOptions));
    }
}
