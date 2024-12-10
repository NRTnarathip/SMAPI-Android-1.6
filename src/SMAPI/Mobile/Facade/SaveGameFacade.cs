using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.TerrainFeatures;

namespace StardewModdingAPI.Mobile.Facade;

public class SaveGameFacade : SaveGame, IRewriteFacade
{
    public static XmlSerializer serializer { get; set; } = new XmlSerializer(typeof(SaveGame), new Type[5]
  {
        typeof(Character),
        typeof(GameLocation),
        typeof(Item),
        typeof(Quest),
        typeof(TerrainFeature)
  });

    public static XmlSerializer farmerSerializer { get; set; } = new XmlSerializer(typeof(Farmer), new Type[1] { typeof(Item) });

    public static XmlSerializer locationSerializer { get; set; } = new XmlSerializer(typeof(GameLocation), new Type[3]
    {
        typeof(Character),
        typeof(Item),
        typeof(TerrainFeature)
    });

    public static XmlSerializer descriptionElementSerializer { get; set; } = new XmlSerializer(typeof(DescriptionElement), new Type[2]
    {
        typeof(Character),
        typeof(Item)
    });

    public static XmlSerializer legacyDescriptionElementSerializer { get; set; } = new XmlSerializer(typeof(SaveMigrator_1_6.LegacyDescriptionElement), new Type[3]
    {
        typeof(DescriptionElement),
        typeof(Character),
        typeof(Item)
    });

}
