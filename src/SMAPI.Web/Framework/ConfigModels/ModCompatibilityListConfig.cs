namespace StardewModdingAPI.Web.Framework.ConfigModels;

/// <summary>The config settings for the mod compatibility list.</summary>
internal class ModCompatibilityListConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>The number of minutes before which compatibility list data should be considered old.</summary>
    public int StaleMinutes { get; set; }
}
