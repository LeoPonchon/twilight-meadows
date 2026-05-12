using NUnit.Framework;

public class GoldWalletTests
{
    [Test]
    public void StartingGold_IsClampedToZero()
    {
        var wallet = new GoldWallet(-10);
        Assert.AreEqual(0, wallet.Gold);
    }

    [Test]
    public void Add_IncreasesGold_AndFiresEvent()
    {
        var wallet = new GoldWallet(0);
        int last = -1;
        wallet.GoldChanged += g => last = g;

        wallet.Add(5);

        Assert.AreEqual(5, wallet.Gold);
        Assert.AreEqual(5, last);
    }

    [Test]
    public void Spend_FailsWhenInsufficient()
    {
        var wallet = new GoldWallet(3);
        Assert.IsFalse(wallet.Spend(4));
        Assert.AreEqual(3, wallet.Gold);
    }
}

