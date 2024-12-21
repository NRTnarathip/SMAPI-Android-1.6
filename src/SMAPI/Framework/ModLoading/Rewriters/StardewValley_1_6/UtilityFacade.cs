using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using SObject = StardewValley.Object;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.5.6's <see cref="Utility"/> methods to their newer form to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
public class UtilityFacade : Utility, IRewriteFacade
{
    /*********
    ** Public methods
    *********/
    /// <remarks>Replaced in Stardew Valley 1.6.15.</remarks>
    public static void checkForBooksReadAchievement()
    {
        Game1.stats.checkForBooksReadAchievement();
    }

    public static bool doesItemWithThisIndexExistAnywhere(int index, bool bigCraftable = false)
    {
        bool found = false;

        Utility.ForEachItem(item =>
        {
            found = item is SObject obj && obj.bigCraftable.Value == bigCraftable && obj.ParentSheetIndex == index;
            return !found;
        });

        return found;
    }

    public static void ForAllLocations(Action<GameLocation> action)
    {
        Utility.ForEachLocation(location =>
        {
            action(location);
            return true;
        });
    }

#pragma warning disable CS0618 // Type or member is obsolete: this is backwards-compatibility code.
    public new static DisposableList<NPC> getAllCharacters()
    {
        return new DisposableList<NPC>(Utility.getAllCharacters());
    }
#pragma warning restore CS0618

    public static List<NPC> getAllCharacters(List<NPC> list)
    {
        list.AddRange(Utility.getAllCharacters());
        return list;
    }

    public new static IEnumerable<int> GetHorseWarpRestrictionsForFarmer(Farmer who)
    {
        Utility.HorseWarpRestrictions restrictions = Utility.GetHorseWarpRestrictionsForFarmer(who);

        if (restrictions.HasFlag(Utility.HorseWarpRestrictions.NoOwnedHorse))
            yield return 1;
        if (restrictions.HasFlag(Utility.HorseWarpRestrictions.Indoors))
            yield return 2;
        if (restrictions.HasFlag(Utility.HorseWarpRestrictions.NoRoom))
            yield return 3;
        if (restrictions.HasFlag(Utility.HorseWarpRestrictions.InUse))
            yield return 3;
    }

    public static T GetRandom<T>(List<T> list, Random? random = null)
    {
        return (random ?? Game1.random).ChooseFrom(list);
    }

    public static NPC? getTodaysBirthdayNPC(string season, int day)
    {
        // use new method if possible
        if (season == Game1.currentSeason && day == Game1.dayOfMonth)
            return Utility.getTodaysBirthdayNPC();

        // else replicate old behavior
        NPC? found = null;
        Utility.ForEachCharacter(npc =>
        {
            if (npc.birthday_Season.Value == season && npc.birthday_Day.Value == day)
                found = npc;

            return found is null;
        });
        return found;
    }

    public static bool HasAnyPlayerSeenEvent(int event_number)
    {
        return Utility.HasAnyPlayerSeenEvent(event_number.ToString());
    }

    public static bool HaveAllPlayersSeenEvent(int event_number)
    {
        return Utility.HaveAllPlayersSeenEvent(event_number.ToString());
    }

    public static bool isFestivalDay(int day, string season)
    {
        return
            Utility.TryParseEnum(season, out Season parsedSeason)
            && Utility.isFestivalDay(day, parsedSeason);
    }

    public static bool IsNormalObjectAtParentSheetIndex(Item item, int index)
    {
        return Utility.IsNormalObjectAtParentSheetIndex(item, index.ToString());
    }

    public static int numObelisksOnFarm()
    {
        return Utility.GetObeliskTypesBuilt();
    }

    public static int numSilos()
    {
        return Game1.GetNumberBuildingsConstructed("Silo");
    }

    public new static List<TemporaryAnimatedSprite> sparkleWithinArea(Rectangle bounds, int numberOfSparkles, Color sparkleColor, int delayBetweenSparkles = 100, int delayBeforeStarting = 0, string sparkleSound = "")
    {
        TemporaryAnimatedSpriteList list = Utility.getTemporarySpritesWithinArea(new[] { 10, 11 }, bounds, numberOfSparkles, sparkleColor, delayBetweenSparkles, delayBeforeStarting, sparkleSound);
        return list.ToList();
    }

    public static void spreadAnimalsAround(Building b, Farm environment)
    {
        Utility.spreadAnimalsAround(b, environment);
    }


    /*********
    ** Private methods
    *********/
    private UtilityFacade()
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
