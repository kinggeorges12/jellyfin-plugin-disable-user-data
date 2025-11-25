using System;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Jellyfin.Plugin.DisableUserData;

public sealed class DisableUserDataActionFilter : IAsyncActionFilter
{
    private readonly ILibraryManager _libraryManager;

    public DisableUserDataActionFilter(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var plugin = Plugin.Instance;
        if (plugin is null || !plugin.Configuration.Enabled)
        {
            await next();
            return;
        }

        var request = context.HttpContext.Request;
        Console.WriteLine($"Disable UserData - {context.Controller}");
        Console.WriteLine($"Disable UserData - {request.Path}");

        // This if is mostly for short-circuiting purposes
        if (DisabledForCollections(context, request)
            || DisabledForContinueWatching(context, request)
            || DisabledForNextUp(context, request)
            || DisabledForRecentlyAdded(context, request))
        {
            await next();
            return;
        }

        await next();
    }

    private bool DisabledForCollections(ActionExecutingContext context, HttpRequest request)
    {
        if (!Plugin.Instance.Configuration.DisableOnCollections)
        {
            return false;
        }

        // Handles cases where the parent is not the collections folder, but collections are included.
        // Applies for things like navigating to Wolphin's Movies, then selecting collections
        if (request.Query.TryGetValue("includeItemTypes", out StringValues includeItemTypes) &&
            includeItemTypes.Contains("BoxSet"))
        {
            DisableUserData(context);
            return true;
        }

        // Handles cases where the parent is the collections folder, such as navigating to collections from the home
        // on Jellyfin web, Jellyfin Media Player, and others
        if (request.Query.TryGetValue("parentId", out StringValues parentIdValues) &&
            Guid.TryParse(parentIdValues[0], out var parentId))
        {
            BaseItem? parent = _libraryManager.GetItemById(parentId);
            if (parent is CollectionFolder)
            {
                DisableUserData(context);
                return true;
            }
        }

        return false;
    }

    private bool DisabledForContinueWatching(ActionExecutingContext context, HttpRequest request)
    {
        if (!Plugin.Instance.Configuration.DisableOnContinueWatching)
        {
            return false;
        }

        if (request.Path.ToString().EndsWith("/Resume", StringComparison.InvariantCultureIgnoreCase))
        {
            DisableUserData(context);
            return true;
        }

        return false;
    }

    private bool DisabledForNextUp(ActionExecutingContext context, HttpRequest request)
    {
        if (!Plugin.Instance.Configuration.DisableOnNextUp)
        {
            return false;
        }

        if (request.Path.ToString().EndsWith("/NextUp", StringComparison.InvariantCultureIgnoreCase))
        {
            DisableUserData(context);
            return true;
        }

        return false;
    }

    private bool DisabledForRecentlyAdded(ActionExecutingContext context, HttpRequest request)
    {
        if (!Plugin.Instance.Configuration.DisableOnRecentlyAdded)
        {
            return false;
        }

        if (request.Path.ToString().EndsWith("/Latest", StringComparison.InvariantCultureIgnoreCase))
        {
            DisableUserData(context);
            return true;
        }

        return false;
    }

    private void DisableUserData(ActionExecutingContext context)
    {
        context.ActionArguments["enableUserData"] = false;
    }
}
