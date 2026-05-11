using System;
using System.Collections.Generic;

namespace RePuzzleKnights.Scripts.Infrastructure.InGame.StageData
{
    [Serializable]
    public class StageMapData
    {
        public string stageName = "NewStage";
        public int width = 10;
        public int height = 10;
        public List<CellData> cells = new List<CellData>();
    }

    [Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public BlockType blockType;
    }

    // 0=Empty 1=Ground 2=HighGround 3=Start 4=Goal 5=NonPlaceable 6=Pitfall
    public enum BlockType
    {
        Empty = 0,
        Ground = 1,
        HighGround = 2,
        Start = 3,
        Goal = 4,
        NonPlaceable = 5,
        Pitfall = 6
    }
}