using System.Collections.Generic;
using StardewModdingAPI.Framework.StateTracking.Comparers;
using StardewValley;

namespace StardewModdingAPI.Framework.StateTracking.Snapshots;

/// <summary>A frozen snapshot of the tracked game locations.</summary>
internal class WorldLocationsSnapshot
{
    /*********
    ** Fields
    *********/
    /// <summary>A map of tracked locations.</summary>
    private readonly Dictionary<GameLocation, LocationSnapshot> LocationsDict = new(new ObjectReferenceComparer<GameLocation>());

    /// <summary>The pooled list instance for <see cref="GetMissingLocations"/>.</summary>
    private static readonly List<GameLocation> PooledMissingLocations = [];


    /*********
    ** Accessors
    *********/
    /// <summary>Tracks changes to the location list.</summary>
    public SnapshotListDiff<GameLocation> LocationList { get; } = new();

    /// <summary>The tracked locations.</summary>
    public IEnumerable<LocationSnapshot> Locations => this.LocationsDict.Values;


    /*********
    ** Public methods
    *********/
    /// <summary>Update the tracked values.</summary>
    /// <param name="watcher">The watcher to snapshot.</param>
    public void Update(WorldLocationsTracker watcher)
    {
        // update location list
        this.LocationList.Update(watcher.IsLocationListChanged, watcher.Added, watcher.Removed);

        // remove missing locations
        foreach (var key in this.GetMissingLocations(watcher))
            this.LocationsDict.Remove(key);

        // update locations
        foreach (LocationTracker locationWatcher in watcher.Locations)
        {
            if (!this.LocationsDict.TryGetValue(locationWatcher.Location, out LocationSnapshot? snapshot))
                this.LocationsDict[locationWatcher.Location] = snapshot = new LocationSnapshot(locationWatcher.Location);

            snapshot.Update(locationWatcher);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the watched locations which no longer exist in the world, if any.</summary>
    /// <param name="watcher">The location list tracker.</param>
    private List<GameLocation> GetMissingLocations(WorldLocationsTracker watcher)
    {
        List<GameLocation> list = WorldLocationsSnapshot.PooledMissingLocations;
        if (list.Count > 0)
            list.Clear();

        foreach (GameLocation location in this.LocationsDict.Keys)
        {
            if (!watcher.HasLocationTracker(location))
                list.Add(location);
        }

        return list;
    }
}
