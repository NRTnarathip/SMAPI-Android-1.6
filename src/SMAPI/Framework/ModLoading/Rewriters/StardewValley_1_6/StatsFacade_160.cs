using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.5.6's <see cref="Stats"/> methods to their newer form to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named due to technical limitations, since we can't have two different facades for the same fields in 1.6.0 and 1.6.15.")]
public class StatsFacade_160 : Stats, IRewriteFacade
{
    /*********
    ** Accessors
    *********/
    /****
    ** Other fields
    ****/
    public new SerializableDictionary<string, int> specificMonstersKilled
    {
        get => base.specificMonstersKilled;
    }

    public SerializableDictionary<string, uint> stat_dictionary
    {
        get => base.Values;
    }

    /****
    ** Former uint fields
    ****/
    public uint averageBedtime
    {
        get => base.AverageBedtime;
        set => base.AverageBedtime = value;
    }

    public uint beveragesMade
    {
        get => base.BeveragesMade;
        set => base.BeveragesMade = value;
    }

    public uint caveCarrotsFound
    {
        get => base.CaveCarrotsFound;
        set => base.CaveCarrotsFound = value;
    }

    public uint cheeseMade
    {
        get => base.CheeseMade;
        set => base.CheeseMade = value;
    }

    public uint chickenEggsLayed
    {
        get => base.ChickenEggsLayed;
        set => base.ChickenEggsLayed = value;
    }

    public uint copperFound
    {
        get => base.CopperFound;
        set => base.CopperFound = value;
    }

    public uint cowMilkProduced
    {
        get => base.CowMilkProduced;
        set => base.CowMilkProduced = value;
    }

    public uint cropsShipped
    {
        get => base.CropsShipped;
        set => base.CropsShipped = value;
    }

    public uint daysPlayed
    {
        get => base.DaysPlayed;
        set => base.DaysPlayed = value;
    }

    public uint diamondsFound
    {
        get => base.DiamondsFound;
        set => base.DiamondsFound = value;
    }

    public uint dirtHoed
    {
        get => base.DirtHoed;
        set => base.DirtHoed = value;
    }

    public uint duckEggsLayed
    {
        get => base.DuckEggsLayed;
        set => base.DuckEggsLayed = value;
    }

    public uint fishCaught
    {
        get => base.FishCaught;
        set => base.FishCaught = value;
    }

    public uint geodesCracked
    {
        get => base.GeodesCracked;
        set => base.GeodesCracked = value;
    }

    public uint giftsGiven
    {
        get => base.GiftsGiven;
        set => base.GiftsGiven = value;
    }

    public uint goatCheeseMade
    {
        get => base.GoatCheeseMade;
        set => base.GoatCheeseMade = value;
    }

    public uint goatMilkProduced
    {
        get => base.GoatMilkProduced;
        set => base.GoatMilkProduced = value;
    }

    public uint goldFound
    {
        get => base.GoldFound;
        set => base.GoldFound = value;
    }

    public uint goodFriends
    {
        get => base.GoodFriends;
        set => base.GoodFriends = value;
    }

    public uint individualMoneyEarned
    {
        get => base.IndividualMoneyEarned;
        set => base.IndividualMoneyEarned = value;
    }

    public uint iridiumFound
    {
        get => base.IridiumFound;
        set => base.IridiumFound = value;
    }

    public uint ironFound
    {
        get => base.IronFound;
        set => base.IronFound = value;
    }

    public uint itemsCooked
    {
        get => base.ItemsCooked;
        set => base.ItemsCooked = value;
    }

    public uint itemsCrafted
    {
        get => base.ItemsCrafted;
        set => base.ItemsCrafted = value;
    }

    public uint itemsForaged
    {
        get => base.ItemsForaged;
        set => base.ItemsForaged = value;
    }

    public uint itemsShipped
    {
        get => base.ItemsShipped;
        set => base.ItemsShipped = value;
    }

    public uint monstersKilled
    {
        get => base.MonstersKilled;
        set => base.MonstersKilled = value;
    }

    public uint mysticStonesCrushed
    {
        get => base.MysticStonesCrushed;
        set => base.MysticStonesCrushed = value;
    }

    public uint notesFound
    {
        get => base.NotesFound;
        set => base.NotesFound = value;
    }

    public uint otherPreciousGemsFound
    {
        get => base.OtherPreciousGemsFound;
        set => base.OtherPreciousGemsFound = value;
    }

    public uint piecesOfTrashRecycled
    {
        get => base.PiecesOfTrashRecycled;
        set => base.PiecesOfTrashRecycled = value;
    }

    public uint preservesMade
    {
        get => base.PreservesMade;
        set => base.PreservesMade = value;
    }

    public uint prismaticShardsFound
    {
        get => base.PrismaticShardsFound;
        set => base.PrismaticShardsFound = value;
    }

    public uint questsCompleted
    {
        get => base.QuestsCompleted;
        set => base.QuestsCompleted = value;
    }

    public uint rabbitWoolProduced
    {
        get => base.RabbitWoolProduced;
        set => base.RabbitWoolProduced = value;
    }

    public uint rocksCrushed
    {
        get => base.RocksCrushed;
        set => base.RocksCrushed = value;
    }

    public uint seedsSown
    {
        get => base.SeedsSown;
        set => base.SeedsSown = value;
    }

    public uint sheepWoolProduced
    {
        get => base.SheepWoolProduced;
        set => base.SheepWoolProduced = value;
    }

    public uint slimesKilled
    {
        get => base.SlimesKilled;
        set => base.SlimesKilled = value;
    }

    public uint stepsTaken
    {
        get => base.StepsTaken;
        set => base.StepsTaken = value;
    }

    public uint stoneGathered
    {
        get => base.StoneGathered;
        set => base.StoneGathered = value;
    }

    public uint stumpsChopped
    {
        get => base.StumpsChopped;
        set => base.StumpsChopped = value;
    }

    public uint timesFished
    {
        get => base.TimesFished;
        set => base.TimesFished = value;
    }

    public uint timesUnconscious
    {
        get => base.TimesUnconscious;
        set => base.TimesUnconscious = value;
    }

    public uint totalMoneyGifted
    {
        get => base.Get("totalMoneyGifted");
        set => base.Set("totalMoneyGifted", value);
    }

    public uint trufflesFound
    {
        get => base.TrufflesFound;
        set => base.TrufflesFound = value;
    }

    public uint weedsEliminated
    {
        get => base.WeedsEliminated;
        set => base.WeedsEliminated = value;
    }


    /*********
    ** Public methods
    *********/
    public uint getStat(string label)
    {
        return base.Get(label);
    }

    public void incrementStat(string label, int amount)
    {
        base.Increment(label, amount);
    }



    /*********
    ** Private methods
    *********/
    private StatsFacade_160()
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
