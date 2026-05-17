public interface IWorldSaveService
{
    WorldStateSaveData CaptureWorld();
    void ApplyWorld(WorldStateSaveData world);
}

