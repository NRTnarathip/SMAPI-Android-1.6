using System.Collections.Generic;
using Netcode;
using StardewValley;
using StardewValley.Inventories;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6.Internal;

/// <summary>An implementation of <see cref="NetObjectList{T}"/> which tracks an underlying <see cref="Inventory"/> instance.</summary>
internal class InventoryToNetObjectList : NetObjectList<Item>
{
    /*********
    ** Fields
    *********/
    /// <summary>A cached lookup of inventory wrappers.</summary>
    private static readonly Dictionary<Inventory, InventoryToNetObjectList> CachedWrappers = new(ReferenceEqualityComparer.Instance);

    /// <summary>The underlying inventory to track.</summary>
    private readonly Inventory Inventory;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="inventory">The underlying inventory to track.</param>
    public InventoryToNetObjectList(Inventory inventory)
    {
        this.Inventory = inventory;

        this.RebuildList();

        this.Inventory.OnInventoryReplaced += this.OnInventoryReplaced;
        this.Inventory.OnSlotChanged += this.OnInventorySlotChanged;
    }

    /// <summary>Get a wrapper for a given inventory instance.</summary>
    /// <param name="inventory">The inventory to track.</param>
    public static InventoryToNetObjectList GetCachedWrapperFor(Inventory inventory)
    {
        if (!CachedWrappers.TryGetValue(inventory, out InventoryToNetObjectList? wrapper))
            CachedWrappers[inventory] = wrapper = new InventoryToNetObjectList(inventory);

        return wrapper;
    }

    /// <inheritdoc />
    public override Item this[int index]
    {
        get => this.Inventory[index];
        set => this.Inventory[index] = value;
    }

    /// <inheritdoc />
    public override void Add(Item item)
    {
        this.Inventory.Add(item);
    }

    /// <inheritdoc />
    public override void Clear()
    {
        this.Inventory.Clear();
    }

    /// <inheritdoc />
    public override void Insert(int index, Item item)
    {
        this.Inventory.Insert(index, item);
    }

    /// <inheritdoc />
    public override void RemoveAt(int index)
    {
        this.Inventory.RemoveAt(index);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Handle a change to the underlying inventory.</summary>
    /// <param name="inventory">The inventory instance.</param>
    /// <param name="index">The slot index which changed.</param>
    /// <param name="before">The previous value.</param>
    /// <param name="after">The new value.</param>
    private void OnInventorySlotChanged(Inventory inventory, int index, Item before, Item after)
    {
        try
        {
            base[index] = after;
        }
        catch
        {
            this.RebuildList(); // if the item list is out of sync, rebuild it
        }
    }

    /// <summary>Handle the underlying inventory getting replaced with a new list.</summary>
    /// <param name="inventory">The inventory instance.</param>
    /// <param name="before">The previous list of values.</param>
    /// <param name="after">The new list of values.</param>
    private void OnInventoryReplaced(Inventory inventory, IList<Item> before, IList<Item> after)
    {
        this.RebuildList();
    }

    /// <summary>Rebuild the list to match the underlying inventory.</summary>
    private void RebuildList()
    {
        // don't use `this` to avoid re-editing the inventory
        base.Clear();
        foreach (Item slot in this.Inventory)
            base.Add(slot);
    }
}
