[System.Serializable]
public class ItemStack
{
    public ItemData itemData;  // Référence à l'objet ScriptableObject
    public int quantity;       // Quantité de cet objet dans l'inventaire

    public ItemStack(ItemData data, int qty = 1)
    {
        itemData = data;
        quantity = qty;
    }
}
