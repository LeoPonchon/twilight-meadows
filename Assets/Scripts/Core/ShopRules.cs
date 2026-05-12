public static class ShopRules
{
    public static bool CanBuy(int playerGold, int buyPrice, bool hasStock, bool inventoryCanAdd)
    {
        if (buyPrice < 0) return false;
        if (!hasStock) return false;
        if (!inventoryCanAdd) return false;
        return playerGold >= buyPrice;
    }

    public static bool CanSell(bool inventoryHasItem)
    {
        return inventoryHasItem;
    }
}

