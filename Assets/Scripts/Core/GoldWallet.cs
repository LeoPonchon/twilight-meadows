using System;

public sealed class GoldWallet
{
    public int Gold { get; private set; }

    public event Action<int> GoldChanged;

    public GoldWallet(int startingGold = 0)
    {
        Gold = Math.Max(0, startingGold);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        GoldChanged?.Invoke(Gold);
    }

    public bool CanSpend(int amount)
    {
        return amount >= 0 && Gold >= amount;
    }

    public bool Spend(int amount)
    {
        if (!CanSpend(amount)) return false;
        Gold -= amount;
        GoldChanged?.Invoke(Gold);
        return true;
    }

    public void Set(int amount)
    {
        Gold = Math.Max(0, amount);
        GoldChanged?.Invoke(Gold);
    }
}

