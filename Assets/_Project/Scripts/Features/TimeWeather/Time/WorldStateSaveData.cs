using System;
using System.Collections.Generic;

[Serializable]
public sealed class WorldStateSaveData
{
    [Serializable]
    public struct Cell
    {
        public int x;
        public int y;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public sealed class Crop
    {
        public int x;
        public int y;

        public string seedName;

        public int currentStage;
        public int dayPlanted;
        public int lastWateredDay;
        public bool isWithered;

        public bool isPerennial;
        public int lastProductionDay;
        public bool hasFruits;
    }

    [Serializable]
    public sealed class PlacedTile
    {
        public int x;
        public int y;
        public string tileName;
    }

    public List<Cell> soils = new List<Cell>();
    public List<Cell> wetSoils = new List<Cell>();
    public List<Crop> crops = new List<Crop>();

    public List<PlacedTile> rocks = new List<PlacedTile>();
    public List<PlacedTile> trees = new List<PlacedTile>();
}
