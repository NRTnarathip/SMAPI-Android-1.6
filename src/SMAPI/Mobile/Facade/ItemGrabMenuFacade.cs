using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley;
using StardewValley.Menus;
using static StardewValley.Menus.InventoryMenu;

namespace StardewModdingAPI.Mobile.Facade;

public class ItemGrabMenuFacade : ItemGrabMenu, IRewriteFacade
{
    public ItemGrabMenuFacade(IList<Item> inventory,
        bool reverseGrab,
        bool showReceivingMenu,
        InventoryMenu.highlightThisItem highlightFunction,
        behaviorOnItemSelect behaviorOnItemSelectFunction,
        string message,
        behaviorOnItemSelect behaviorOnItemGrab = null,
        bool snapToBottom = false,
        bool canBeExitedWithKey = false,
        bool playRightClickSound = true,
        bool allowRightClick = true,
        bool showOrganizeButton = false,
        int source = 0,
        Item sourceItem = null,
        int whichSpecialButton = -1,
        object context = null,
        ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer,
        bool allowExitWithHeldItem = false)
            : base(
                inventory: inventory,
                reverseGrab: reverseGrab,
                showReceivingMenu: showReceivingMenu,
                highlightFunction: highlightFunction,
                behaviorOnItemSelectFunction, message: message,
                behaviorOnItemGrab,
                snapToBottom,
                canBeExitedWithKey,
                playRightClickSound,
                allowRightClick,
                showOrganizeButton,
                source,
                sourceItem,
                whichSpecialButton)
    {

    }

    //public ItemGrabMenu(IList<Item> inventory,
    //    bool reverseGrab,
    //    bool showReceivingMenu,
    //InventoryMenu.highlightThisItem highlightFunction,
    //behaviorOnItemSelect behaviorOnItemSelectFunction,
    //string message,
    //behaviorOnItemSelect behaviorOnItemGrab = null,
    //bool snapToBottom = false,
    //bool canBeExitedWithKey = false,
    //bool playRightClickSound = true,
    //bool allowRightClick = true,
    //bool showOrganizeButton = false,
    //int source = 0,
    //Item sourceItem = null,
    //int whichSpecialButton = -1,
    //object specialObject = null,
    //int storageCapacity = -1,
    //int numRows = 3,
    //behaviorOnMobileItemChange itemChangeBehavior = null,
    //bool allowStack = true,
    //behaviorOnItemAddtoItemsToGrab behaviorOnAddtoTop = null,
    //bool rearrangeGrangeOnExit = false,
    //behaviorOnTapClose behaviorOnTapClose = null,
    //object context = null,
    //bool allowExitWithHeldItem = false)
    //{

    //}

    void Test()
    {
        //MethodInfo method = typeof(Farm).GetMethod("shipItem");
        //ItemExitBehavior itemExitBehavior = ItemExitBehavior.ReturnToPlayer;
        //ItemGrabMenuFacade itemGrabMenu = new((IList<Item>)null,
        //    true,
        //    false,
        //    new highlightThisItem(Utility.highlightShippableObjects),
        //    (behaviorOnItemSelect)Delegate.CreateDelegate(typeof(behaviorOnItemSelect), Game1.getFarm(), method),
        //    "",
        //    (behaviorOnItemSelect)null,
        //    true,
        //    true,
        //    false,
        //    true,
        //    false,
        //    0,
        //    (Item)null,
        //    -1,
        //    null,
        //    itemExitBehavior,
        //    false);

    }

}
