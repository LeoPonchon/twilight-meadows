using NUnit.Framework;

public class InventoryCoreTests
{
    private sealed class Item
    {
        public Item(string id, bool stackable)
        {
            Id = id;
            Stackable = stackable;
        }

        public string Id { get; }
        public bool Stackable { get; }
    }

    private sealed class Stack
    {
        public Stack(Item item, int qty)
        {
            Item = item;
            Quantity = qty;
        }

        public Item Item { get; }
        public int Quantity { get; set; }
    }

    [Test]
    public void Add_Stacks_WhenItemIsStackable()
    {
        var item = new Item("apple", stackable: true);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 2,
            inventorySlots: 2,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 1);
        inv.Add(item, 2);

        Assert.AreEqual(1, inv.Slots.Count);
        Assert.AreEqual(3, inv.GetQuantityTotal(item));
    }

    [Test]
    public void Add_PrefersInventoryRange_BeforeHotbarFallback()
    {
        var item = new Item("stone", stackable: false);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 2,
            inventorySlots: 2,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 1);
        Assert.IsNotNull(inv.GetInSlot(2));
    }

    [Test]
    public void Remove_RemovesSlot_WhenQuantityReachesZero()
    {
        var item = new Item("apple", stackable: true);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 1,
            inventorySlots: 1,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 2);
        inv.Remove(item, 2);

        Assert.AreEqual(0, inv.Slots.Count);
    }

    [Test]
    public void Remove_DecrementsQuantity_WhenQuantityRemains()
    {
        var item = new Item("apple", stackable: true);
        var inv = new InventoryCore<Item, Stack>(
            hotbarSlots: 1,
            inventorySlots: 1,
            getItem: s => s.Item,
            getQuantity: s => s.Quantity,
            setQuantity: (s, q) => s.Quantity = q,
            isStackable: i => i.Stackable,
            createStack: (i, q) => new Stack(i, q));

        inv.Add(item, 5);
        inv.Remove(item, 2);

        Assert.AreEqual(1, inv.Slots.Count);
        Assert.AreEqual(3, inv.GetQuantityTotal(item));
    }
}
