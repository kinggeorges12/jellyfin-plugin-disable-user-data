using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DisableUserData.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        Enabled = true;
        DisableOnCollections = false;
        DisableOnNextUp = false;
        DisableOnContinueWatching = false;
        DisableOnRecentlyAdded = false;
    }

    /// <summary>
    /// Global toggle: completely enable/disable the plugin.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Disable UserData for collections on Items endpoints.
    /// (GetItems + GetItemsByUserIdLegacy).
    /// </summary>
    public bool DisableOnCollections { get; set; }

    /// <summary>
    /// Disable UserData on Continue Watching endpoint
    /// (GetResumeItemsLegacy).
    /// </summary>
    public bool DisableOnContinueWatching { get; set; }

    /// <summary>
    /// Disable UserData on NextUp endpoint
    /// (GetNextUp).
    /// </summary>
    public bool DisableOnNextUp { get; set; }

    /// <summary>
    /// Disable UserData on Recently Added endpoints
    /// (GetResumeItemsLegacy + GetResumeItems).
    /// </summary>
    public bool DisableOnRecentlyAdded { get; set; }
}
