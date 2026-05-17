public static class SaveLoadState
{
    public static bool HasLoadedWorldState { get; set; }

    public static void ResetForNewSession()
    {
        HasLoadedWorldState = false;
    }
}

