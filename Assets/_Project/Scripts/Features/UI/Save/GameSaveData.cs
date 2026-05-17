using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
    public string slotId;
    public string savedAtUtc;

    public string sceneName;

    public Vector3 playerPosition;
    public bool hasPlayerPosition;

    public int gold;

    public int day;
    public int seasonId;
    public int year;
    public int hour;
    public int minute;

    public WorldStateSaveData world;
}
