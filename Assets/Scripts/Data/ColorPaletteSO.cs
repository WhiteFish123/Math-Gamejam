using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameCore
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Color Palette")]
    public class ColorPaletteSO : ScriptableObject
    {
        [Serializable]
        public struct ColorEntry
        {
            public BoxColor color;
            public TileBase floorTile;
            public Sprite boxSprite;
        }

        public ColorEntry[] entries;

        public TileBase GetFloorTile(BoxColor color)
        {
            foreach (var e in entries)
                if (e.color == color) return e.floorTile;
            return null;
        }

        public Sprite GetBoxSprite(BoxColor color)
        {
            foreach (var e in entries)
                if (e.color == color) return e.boxSprite;
            return null;
        }
    }
}
