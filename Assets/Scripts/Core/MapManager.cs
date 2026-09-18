using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameCore
{
    public class MapManager
    {
        private int width, height;
        private Tilemap floorTilemap;
        private Tilemap antiStainTilemap;
        private LayerMask wallLayer;
        private Grid grid;
        private ColorPaletteSO palette;

        public bool showDebug = true;

        public TileType[,] tileTypes;
        public int[,] colorGrid;

        public int Width => width;
        public int Height => height;

        public TileType GetTileType(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return TileType.Wall;
            return tileTypes[x, y];
        }

        public void Init(LevelConfig config)
        {
            width = config.gridWidth;
            height = config.gridHeight;
            floorTilemap = config.floorTilemap;
            antiStainTilemap = config.specialTilemap;
            wallLayer = config.wallLayer;
            grid = config.grid;
            palette = config.colorPalette;

            tileTypes = new TileType[width, height];
            colorGrid = new int[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (antiStainTilemap != null && antiStainTilemap.HasTile(cell))
                    tileTypes[x, y] = TileType.AntiStain;
                else if (IsWallAt(new Vector2Int(x, y)))
                    tileTypes[x, y] = TileType.Wall;
                else if (floorTilemap != null && floorTilemap.HasTile(cell))
                    tileTypes[x, y] = TileType.Floor;
                else
                    tileTypes[x, y] = TileType.Void;

                colorGrid[x, y] = 0;
            }
        }

        bool IsWallAt(Vector2Int cell)
        {
            Vector3 worldPos = grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
            Vector2 size = grid.cellSize * 0.8f;
            return Physics2D.OverlapBox(worldPos, size, 0f, wallLayer) != null;
        }

        public bool IsWalkable(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            {
                if (showDebug) Debug.Log($"[MapManager] 越界：cell=({cell.x},{cell.y}), 地图范围=[0~{width-1}, 0~{height-1}]");
                return false;
            }
            var type = tileTypes[cell.x, cell.y];
            if (type == TileType.Wall || type == TileType.Void)
                if (showDebug) Debug.Log($"[MapManager] 不可通行：cell=({cell.x},{cell.y}), tileType={type}");
            return type != TileType.Wall && type != TileType.Void;
        }

        public bool IsAntiStain(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                return false;
            return tileTypes[cell.x, cell.y] == TileType.AntiStain;
        }

        public void SetColor(Vector2Int cell, BoxColor color)
        {
            if (showDebug) Debug.Log($"[MapManager.SetColor] 请求染色 cell=({cell.x},{cell.y}) 颜色={color}, tileType={((cell.x>=0&&cell.x<width&&cell.y>=0&&cell.y<height)?tileTypes[cell.x,cell.y].ToString():"OOB")}");
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return;
            if (tileTypes[cell.x, cell.y] == TileType.Wall) return;
            if (tileTypes[cell.x, cell.y] == TileType.AntiStain) { if (showDebug) Debug.Log($"[MapManager.SetColor] 跳过：防染色地板"); return; }
            if (tileTypes[cell.x, cell.y] == TileType.Void) { if (showDebug) Debug.Log($"[MapManager.SetColor] 跳过：空白区域"); return; }

            Vector3Int c = new Vector3Int(cell.x, cell.y, 0);
            Vector3 worldPos = grid.GetCellCenterWorld(c);
            if (showDebug) Debug.Log($"[MapManager.SetColor] 实际 SetTile cell=({c.x},{c.y},{c.z}), worldPos={worldPos}, color={color}");
            colorGrid[cell.x, cell.y] = (int)color;
            TileBase tile = palette.GetFloorTile(color);
            if (tile != null && floorTilemap != null)
                floorTilemap.SetTile(c, tile);
        }

        public BoxColor GetColor(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
                return BoxColor.None;
            return (BoxColor)colorGrid[cell.x, cell.y];
        }
    }
}