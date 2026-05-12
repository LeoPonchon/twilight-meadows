using NUnit.Framework;

public sealed class ShopRulesTests
{
    [Test]
    public void CanBuy_False_WhenNotEnoughGold()
    {
        Assert.IsFalse(ShopRules.CanBuy(playerGold: 5, buyPrice: 10, hasStock: true, inventoryCanAdd: true));
    }

    [Test]
    public void CanBuy_False_WhenNoStock()
    {
        Assert.IsFalse(ShopRules.CanBuy(playerGold: 999, buyPrice: 10, hasStock: false, inventoryCanAdd: true));
    }

    [Test]
    public void CanBuy_False_WhenInventoryCannotAdd()
    {
        Assert.IsFalse(ShopRules.CanBuy(playerGold: 999, buyPrice: 10, hasStock: true, inventoryCanAdd: false));
    }

    [Test]
    public void CanBuy_True_WhenAllConditionsMet()
    {
        Assert.IsTrue(ShopRules.CanBuy(playerGold: 10, buyPrice: 10, hasStock: true, inventoryCanAdd: true));
    }
}

