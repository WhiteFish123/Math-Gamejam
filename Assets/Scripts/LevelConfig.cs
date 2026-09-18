using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameCore
{
    [Serializable]
    public struct ColorCount
    {
        public BoxColor color;
        public int count;
    }

    [Serializable]
    public class ColorCountGroup
    {
        public List<ColorCount> entries = new List<ColorCount>();
    }

    public class LevelConfig : MonoBehaviour
    {
        public int gridWidth;
        public int gridHeight;
        public LayerMask wallLayer;
        public Tilemap floorTilemap;
        public Tilemap specialTilemap;
        public ColorPaletteSO colorPalette;
        public Grid grid;

        public ColorCountGroup[] rowConditions;
        public ColorCountGroup[] colConditions;
    }
}