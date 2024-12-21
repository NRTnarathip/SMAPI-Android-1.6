using System;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Serialization.Converters;

namespace StardewModdingAPI.Framework.Networking;

/// <summary>Metadata about the game, SMAPI, and installed mods exchanged with connected computers.</summary>
internal class RemoteContextModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether this player is the host player.</summary>
    public bool IsHost { get; }

    /// <summary>The game's platform.</summary>
    public GamePlatform Platform { get; }

    /// <summary>The installed version of Stardew Valley.</summary>
    [JsonConverter(typeof(NonStandardSemanticVersionConverter))] // versions from Android players may have a fourth number for the platform-specific build
    public ISemanticVersion? GameVersion { get; }

    /// <summary>The installed version of SMAPI.</summary>
    [JsonConverter(typeof(NonStandardSemanticVersionConverter))]
    public ISemanticVersion? ApiVersion { get; }

    /// <summary>The installed mods.</summary>
    public RemoteContextModModel[] Mods { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="isHost">Whether this player is the host player.</param>
    /// <param name="platform">The game's platform.</param>
    /// <param name="gameVersion">The installed version of Stardew Valley.</param>
    /// <param name="apiVersion">The installed version of SMAPI.</param>
    /// <param name="mods">The installed mods.</param>
    public RemoteContextModel(bool isHost, GamePlatform platform, ISemanticVersion gameVersion, ISemanticVersion apiVersion, RemoteContextModModel[]? mods)
    {
        this.IsHost = isHost;
        this.Platform = platform;
        this.GameVersion = gameVersion;
        this.ApiVersion = apiVersion;
        this.Mods = mods ?? Array.Empty<RemoteContextModModel>();
    }
}
