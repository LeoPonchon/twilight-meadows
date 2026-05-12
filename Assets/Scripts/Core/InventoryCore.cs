using System;
using System.Collections.Generic;

public sealed class InventoryCore<TItem, TStack>
    where TItem : class
    where TStack : class
{
    private readonly int capacity;
    private readonly int hotbarSlots;
    private readonly Func<TStack, TItem> getItem;
    private readonly Func<TStack, int> getQuantity;
    private readonly Action<TStack, int> setQuantity;
    private readonly Func<TItem, bool> isStackable;
    private readonly Func<TItem, int, TStack> createStack;
    private readonly IEqualityComparer<TItem> comparer;

    private readonly Dictionary<int, TStack> slots = new Dictionary<int, TStack>();

    public event Action Changed;

    public InventoryCore(
        int hotbarSlots,
        int inventorySlots,
        Func<TStack, TItem> getItem,
        Func<TStack, int> getQuantity,
        Action<TStack, int> setQuantity,
        Func<TItem, bool> isStackable,
        Func<TItem, int, TStack> createStack,
        IEqualityComparer<TItem> comparer = null)
    {
        if (hotbarSlots < 0) throw new ArgumentOutOfRangeException(nameof(hotbarSlots));
        if (inventorySlots < 0) throw new ArgumentOutOfRangeException(nameof(inventorySlots));
        this.hotbarSlots = hotbarSlots;
        capacity = hotbarSlots + inventorySlots;
        this.getItem = getItem ?? throw new ArgumentNullException(nameof(getItem));
        this.getQuantity = getQuantity ?? throw new ArgumentNullException(nameof(getQuantity));
        this.setQuantity = setQuantity ?? throw new ArgumentNullException(nameof(setQuantity));
        this.isStackable = isStackable ?? throw new ArgumentNullException(nameof(isStackable));
        this.createStack = createStack ?? throw new ArgumentNullException(nameof(createStack));
        this.comparer = comparer ?? EqualityComparer<TItem>.Default;
    }

    public IReadOnlyDictionary<int, TStack> Slots => slots;

    public bool CanAdd(TItem item, int quantity)
    {
        if (item == null || quantity <= 0) return false;

        if (isStackable(item))
        {
            foreach (var kvp in slots)
            {
                if (comparer.Equals(getItem(kvp.Value), item)) return true;
            }
        }

        int required = isStackable(item) ? 1 : quantity;
        return slots.Count + required <= capacity;
    }

    public void Add(TItem item, int quantity)
    {
        if (item == null || quantity <= 0) return;

        if (isStackable(item))
        {
            foreach (var kvp in slots)
            {
                var stack = kvp.Value;
                if (!comparer.Equals(getItem(stack), item)) continue;
                setQuantity(stack, getQuantity(stack) + quantity);
                Changed?.Invoke();
                return;
            }
        }

        AddToFirstFreeSlot(item, quantity, hotbarSlots, capacity);
    }

    public void AddToSlot(int slotIndex, TItem item, int quantity)
    {
        if (item == null || quantity <= 0) return;
        if (slotIndex < 0 || slotIndex >= capacity) return;

        if (slots.TryGetValue(slotIndex, out var existing))
        {
            if (isStackable(item) && comparer.Equals(getItem(existing), item))
            {
                setQuantity(existing, getQuantity(existing) + quantity);
                Changed?.Invoke();
            }
            return;
        }

        slots[slotIndex] = createStack(item, quantity);
        Changed?.Invoke();
    }

    public void Remove(TItem item, int quantity)
    {
        if (item == null || quantity <= 0) return;

        int slotIndex = -1;
        TStack foundStack = null;
        foreach (var kvp in slots)
        {
            var stack = kvp.Value;
            if (!comparer.Equals(getItem(stack), item)) continue;
            slotIndex = kvp.Key;
            foundStack = stack;
            break;
        }

        if (slotIndex < 0) return;

        int newQty = getQuantity(foundStack) - quantity;
        if (newQty <= 0) slots.Remove(slotIndex);
        else setQuantity(foundStack, newQty);

        Changed?.Invoke();
    }

    public void RemoveFromSlot(int slotIndex, int quantity)
    {
        if (quantity <= 0) return;
        if (slotIndex < 0 || slotIndex >= capacity) return;

        if (!slots.TryGetValue(slotIndex, out var stack)) return;
        int newQty = getQuantity(stack) - quantity;
        if (newQty <= 0) slots.Remove(slotIndex);
        else setQuantity(stack, newQty);
        Changed?.Invoke();
    }

    public bool Has(TItem item, int quantity)
    {
        if (item == null || quantity <= 0) return false;
        int total = 0;
        foreach (var kvp in slots)
        {
            if (!comparer.Equals(getItem(kvp.Value), item)) continue;
            total += getQuantity(kvp.Value);
            if (total >= quantity) return true;
        }
        return false;
    }

    public int GetQuantityTotal(TItem item)
    {
        if (item == null) return 0;
        int total = 0;
        foreach (var kvp in slots)
        {
            if (!comparer.Equals(getItem(kvp.Value), item)) continue;
            total += getQuantity(kvp.Value);
        }
        return total;
    }

    public TStack GetInSlot(int slotIndex)
    {
        return slots.TryGetValue(slotIndex, out var stack) ? stack : null;
    }

    public Dictionary<int, TStack> Snapshot()
    {
        return new Dictionary<int, TStack>(slots);
    }

    private void AddToFirstFreeSlot(TItem item, int quantity, int startSlot, int endSlotExclusive)
    {
        for (int i = startSlot; i < endSlotExclusive; i++)
        {
            if (slots.ContainsKey(i)) continue;
            slots[i] = createStack(item, quantity);
            Changed?.Invoke();
            return;
        }

        if (startSlot == hotbarSlots)
        {
            AddToFirstFreeSlot(item, quantity, 0, hotbarSlots);
        }
    }
}
