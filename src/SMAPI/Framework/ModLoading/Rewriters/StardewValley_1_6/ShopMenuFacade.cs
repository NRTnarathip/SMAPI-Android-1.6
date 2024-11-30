using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewModdingAPI.Mobile.Facade;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.Objects;
using ShopTabClickableTextureComponent = StardewModdingAPI.Mobile.Facade.ShopTabClickableTextureComponentFacade;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;



/// <summary>Maps Stardew Valley 1.5.6's <see cref="ShopMenu"/> methods to their newer form to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>

public class ShopMenuFacade : ShopMenu, IRewriteFacade
{

    /*********
    ** Private methods
    *********/
    private ShopMenuFacade()
        : base(null, null, null)
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }


#if SMAPI_FOR_ANDROID

    List<ShopTabClickableTextureComponentFacade> _tabButtons = new();
    public List<ShopTabClickableTextureComponentFacade> tabButtons
    {
        get => this._tabButtons;
        set => this._tabButtons = value;
    }

    /// <summary>Add the filter tabs for a catalogue (e.g. flooring and wallpaper).</summary>
    public void UseCatalogueTabs()
    {
        this.tabButtons = new List<ShopTabClickableTextureComponentFacade>
        {
            new ShopTabClickableTextureComponentFacade(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
            {
                myID = 99999,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => true
            },
            new ShopTabClickableTextureComponentFacade(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 64, 16, 16), 4f)
            {
                myID = 100000,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Wallpaper wallpaper2 && wallpaper2.isFloor.Value
            },
            new ShopTabClickableTextureComponentFacade(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 64, 16, 16), 4f)
            {
                myID = 100001,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Wallpaper wallpaper && !wallpaper.isFloor.Value
            }
        };
        this.repositionTabs();
    }

    /// <summary>Add the filter tabs for a furniture catalogue (e.g. tables, seats, paintings, etc).</summary>
	public void UseFurnitureCatalogueTabs()
    {
        this.tabButtons = new List<ShopTabClickableTextureComponent>
        {
            new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
            {
                myID = 99999,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable _) => true
            },
            new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 48, 16, 16), 4f)
            {
                myID = 100000,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Furniture furniture5 && (furniture5.IsTable() || furniture5.furniture_type.Value == 4)
            },
            new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 48, 16, 16), 4f)
            {
                myID = 100001,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Furniture furniture4 && (furniture4.furniture_type.Value == 0 || furniture4.furniture_type.Value == 1 || furniture4.furniture_type.Value == 2 || furniture4.furniture_type.Value == 3)
            },
            new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 64, 16, 16), 4f)
            {
                myID = 100002,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Furniture furniture3 && (furniture3.furniture_type.Value == 6 || furniture3.furniture_type.Value == 13)
            },
            new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 64, 16, 16), 4f)
            {
                myID = 100003,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Furniture furniture2 && furniture2.furniture_type.Value == 12
            },
            new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 64, 16, 16), 4f)
            {
                myID = 100004,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = 3546,
                Filter = (ISalable item) => item is Furniture furniture && (furniture.furniture_type.Value == 7 || furniture.furniture_type.Value == 17 || furniture.furniture_type.Value == 10 || furniture.furniture_type.Value == 8 || furniture.furniture_type.Value == 9 || furniture.furniture_type.Value == 14)
            }
        };
        this.repositionTabs();
    }

    /// <summary>Remove the filter tabs, if any.</summary>
    public void UseNoTabs()
    {
        this.tabButtons = new List<ShopTabClickableTextureComponent>();
        this.repositionTabs();
    }

#else
    /*********
    ** Accessors
    *********/
    public string storeContext
    {
        get => base.ShopId;
        set => base.ShopId = value;
    }


    /*********
    ** Public methods
    *********/
    /// <remarks>Changed in 1.6.0.</remarks>
    public static ShopMenu Constructor(Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string? who = null, Func<ISalable, Farmer, int, bool>? on_purchase = null, Func<ISalable, bool>? on_sell = null, string? context = null)
    {
        return new ShopMenu(ShopMenuFacade.GetShopId(context), ShopMenuFacade.ToItemStockInformation(itemPriceAndStock), currency, who, ToOnPurchaseDelegate(on_purchase), on_sell, playOpenSound: true);
    }

    /// <remarks>Changed in 1.6.0.</remarks>
    public static ShopMenu Constructor(List<ISalable> itemsForSale, int currency = 0, string? who = null, Func<ISalable, Farmer, int, bool>? on_purchase = null, Func<ISalable, bool>? on_sell = null, string? context = null)
    {
        return new ShopMenu(ShopMenuFacade.GetShopId(context), itemsForSale, currency, who, ToOnPurchaseDelegate(on_purchase), on_sell, playOpenSound: true);
    }

    /// <remarks>Changed in 1.6.9.</remarks>
    public static ShopMenu Constructor(string shopId, ShopData shopData, ShopOwnerData ownerData, NPC? owner = null, Func<ISalable, Farmer, int, bool>? onPurchase = null, Func<ISalable, bool>? onSell = null, bool playOpenSound = true)
    {
        return new ShopMenu(shopId, shopData, ownerData, owner, ToOnPurchaseDelegate(onPurchase), onSell, playOpenSound: true);
    }

    /// <remarks>Changed in 1.6.9.</remarks>
    public static ShopMenu Constructor(string shopId, Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, int currency = 0, string? who = null, Func<ISalable, Farmer, int, bool>? on_purchase = null, Func<ISalable, bool>? on_sell = null, bool playOpenSound = true)
    {
        return new ShopMenu(shopId, itemPriceAndStock, currency, who, ToOnPurchaseDelegate(on_purchase), on_sell, playOpenSound);
    }

    /// <remarks>Changed in 1.6.9.</remarks>
    public static ShopMenu Constructor(string shopId, List<ISalable> itemsForSale, int currency = 0, string? who = null, Func<ISalable, Farmer, int, bool>? on_purchase = null, Func<ISalable, bool>? on_sell = null, bool playOpenSound = true)
    {
        return new ShopMenu(shopId, itemsForSale, currency, who, ToOnPurchaseDelegate(on_purchase), on_sell, playOpenSound);
    }

    
    private static string GetShopId(string? context)
    {
        return string.IsNullOrWhiteSpace(context)
            ? "legacy_mod_code_" + Guid.NewGuid().ToString("N")
            : context;
    }

    private static Dictionary<ISalable, ItemStockInformation> ToItemStockInformation(Dictionary<ISalable, int[]>? itemPriceAndStock)
    {
        Dictionary<ISalable, ItemStockInformation> stock = new();

        if (itemPriceAndStock != null)
        {
            foreach (var pair in itemPriceAndStock)
                stock[pair.Key] = new ItemStockInformation(pair.Value[0], pair.Value[1]);
        }

        return stock;
    }

    /// <summary>Convert a pre-1.6.9 <see cref="ShopMenu.onPurchase"/> callback into its new delegate type.</summary>
    /// <param name="onPurchase">The callback to convert.</param>
    [return: NotNullIfNotNull(nameof(onPurchase))]
    private static ShopMenu.OnPurchaseDelegate? ToOnPurchaseDelegate(Func<ISalable, Farmer, int, bool>? onPurchase)
    {
        return onPurchase != null
            ? (item, who, countTaken, _) => onPurchase(item, who, countTaken)
            : null;
    }
#endif
}

