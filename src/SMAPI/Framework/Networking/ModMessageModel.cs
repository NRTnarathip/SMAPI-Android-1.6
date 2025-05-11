using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI.Framework.Networking;

/// <summary>The metadata for a mod message.</summary>
internal class ModMessageModel
{
    /*********
    ** Accessors
    *********/
    /****
    ** Origin
    ****/
    /// <summary>The unique ID of the player who broadcast the message.</summary>
    public long FromPlayerId { get; }

    /// <summary>The unique ID of the mod which broadcast the message.</summary>
    public string FromModId { get; }

    /****
    ** Destination
    ****/
    /// <summary>The players who should receive the message.</summary>
    public long[]? ToPlayerIds { get; init; }

    /// <summary>The mods which should receive the message, or <c>null</c> for all mods.</summary>
    public string[]? ToModIds { get; }

    /// <summary>A message type which receiving mods can use to decide whether it's the one they want to handle, like <c>SetPlayerLocation</c>. This doesn't need to be globally unique, since mods should check the originating mod ID.</summary>
    public string Type { get; }

    /// <summary>The custom mod data being broadcast.</summary>
    public JToken Data { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="fromPlayerId">The unique ID of the player who broadcast the message.</param>
    /// <param name="fromModId">The unique ID of the mod which broadcast the message.</param>
    /// <param name="toPlayerIds">The players who should receive the message, or <c>null</c> for all players.</param>
    /// <param name="toModIds">The mods which should receive the message, or <c>null</c> for all mods.</param>
    /// <param name="type">A message type which receiving mods can use to decide whether it's the one they want to handle, like <c>SetPlayerLocation</c>. This doesn't need to be globally unique, since mods should check the originating mod ID.</param>
    /// <param name="data">The custom mod data being broadcast.</param>
    [JsonConstructor]
    public ModMessageModel(long fromPlayerId, string fromModId, long[]? toPlayerIds, string[]? toModIds, string type, JToken data)
    {
        this.FromPlayerId = fromPlayerId;
        this.FromModId = fromModId;
        this.ToPlayerIds = toPlayerIds;
        this.ToModIds = toModIds;
        this.Type = type;
        this.Data = data;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="message">The message to clone.</param>
    public ModMessageModel(ModMessageModel message)
    {
        this.FromPlayerId = message.FromPlayerId;
        this.FromModId = message.FromModId;
        this.ToPlayerIds = message.ToPlayerIds?.ToArray();
        this.ToModIds = message.ToModIds?.ToArray();
        this.Type = message.Type;
        this.Data = message.Data;
    }
}
