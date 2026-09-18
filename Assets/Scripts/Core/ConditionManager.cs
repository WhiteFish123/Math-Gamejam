using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class ConditionManager
    {
        public ColorCountGroup[] rowConditions;
        public ColorCountGroup[] colConditions;

        private MapManager map;

        public void Init(LevelConfig config, MapManager mapManager)
        {
            rowConditions = config.rowConditions;
            colConditions = config.colConditions;
            map = mapManager;
        }

        public bool CheckWin()
        {
            return CheckAllRows() && CheckAllCols();
        }

        bool CheckAllRows()
        {
            int w = map.tileTypes.GetLength(0);
            int h = map.tileTypes.GetLength(1);
            if (rowConditions == null) return true;
            for (int y = 0; y < h; y++)
            {
                if (y < rowConditions.Length && rowConditions[y] != null
                    && rowConditions[y].entries.Count > 0)
                {
                    if (!CheckLine(CellsInRow(y, w), rowConditions[y].entries))
                        return false;
                }
            }
            return true;
        }

        bool CheckAllCols()
        {
            int w = map.tileTypes.GetLength(0);
            int h = map.tileTypes.GetLength(1);
            if (colConditions == null) return true;
            for (int x = 0; x < w; x++)
            {
                if (x < colConditions.Length && colConditions[x] != null
                    && colConditions[x].entries.Count > 0)
                {
                    if (!CheckLine(CellsInCol(x, h), colConditions[x].entries))
                        return false;
                }
            }
            return true;
        }

        List<BoxColor> CellsInRow(int y, int w)
        {
            var line = new List<BoxColor>();
            for (int x = 0; x < w; x++)
            {
                var t = map.tileTypes[x, y];
                line.Add(t == TileType.Wall || t == TileType.AntiStain || t == TileType.Void
                    ? BoxColor.None : map.GetColor(new Vector2Int(x, y)));
            }
            return line;
        }

        List<BoxColor> CellsInCol(int x, int h)
        {
            var line = new List<BoxColor>();
            for (int y = 0; y < h; y++)
            {
                var t = map.tileTypes[x, y];
                line.Add(t == TileType.Wall || t == TileType.AntiStain || t == TileType.Void
                    ? BoxColor.None : map.GetColor(new Vector2Int(x, y)));
            }
            return line;
        }

        bool CheckLine(List<BoxColor> line, List<ColorCount> conditions)
        {
            var segments = FindSegments(line);
            bool[] segUsed = new bool[segments.Count];
            return MatchSegments(segments, segUsed, conditions, 0, line);
        }

        struct Segment
        {
            public BoxColor color;
            public int length;
        }

        List<Segment> FindSegments(List<BoxColor> line)
        {
            var result = new List<Segment>();
            int i = 0;
            while (i < line.Count)
            {
                if (line[i] == BoxColor.None) { i++; continue; }
                int start = i;
                while (i < line.Count && line[i] == line[start]) i++;
                result.Add(new Segment { color = line[start], length = i - start });
            }
            return result;
        }

        bool MatchSegments(List<Segment> segs, bool[] used, List<ColorCount> conds, int condIdx, List<BoxColor> line)
        {
            if (condIdx >= conds.Count)
            {
                foreach (var cc in conds)
                {
                    int total = 0;
                    foreach (var c in line)
                        if (c == cc.color) total++;
                    int expected = 0;
                    foreach (var c2 in conds)
                        if (c2.color == cc.color) expected += c2.count;
                    if (total != expected) return false;
                }
                return true;
            }
            var target = conds[condIdx];
            for (int i = 0; i < segs.Count; i++)
            {
                if (used[i]) continue;
                if (segs[i].color == target.color && segs[i].length == target.count)
                {
                    used[i] = true;
                    if (MatchSegments(segs, used, conds, condIdx + 1, line))
                        return true;
                    used[i] = false;
                }
            }
            return false;
        }
        public bool IsRowSatisfied(int y)
        {
            if (rowConditions == null || y < 0 || y >= rowConditions.Length || rowConditions[y] == null)
                return true;
            if (rowConditions[y].entries.Count == 0)
                return true;
            int w = map.tileTypes.GetLength(0);
            return CheckLine(CellsInRow(y, w), rowConditions[y].entries);
        }

        public bool IsColSatisfied(int x)
        {
            if (colConditions == null || x < 0 || x >= colConditions.Length || colConditions[x] == null)
                return true;
            if (colConditions[x].entries.Count == 0)
                return true;
            int h = map.tileTypes.GetLength(1);
            return CheckLine(CellsInCol(x, h), colConditions[x].entries);
        }

        public bool IsRowEntrySatisfied(int y, int entryIndex)
        {
            if (rowConditions == null || y < 0 || y >= rowConditions.Length || rowConditions[y] == null)
                return false;
            if (entryIndex < 0 || entryIndex >= rowConditions[y].entries.Count)
                return false;
            var entry = rowConditions[y].entries[entryIndex];
            int w = map.tileTypes.GetLength(0);
            return IsSingleEntrySatisfied(CellsInRow(y, w), entry.color, entry.count);
        }

        public bool IsColEntrySatisfied(int x, int entryIndex)
        {
            if (colConditions == null || x < 0 || x >= colConditions.Length || colConditions[x] == null)
                return false;
            if (entryIndex < 0 || entryIndex >= colConditions[x].entries.Count)
                return false;
            var entry = colConditions[x].entries[entryIndex];
            int h = map.tileTypes.GetLength(1);
            return IsSingleEntrySatisfied(CellsInCol(x, h), entry.color, entry.count);
        }

        private bool IsSingleEntrySatisfied(List<BoxColor> line, BoxColor color, int requiredCount)
        {
            int total = 0;
            foreach (var c in line)
                if (c == color) total++;
            if (total < requiredCount) return false;

            var segments = FindSegments(line);
            int maxLen = 0;
            foreach (var seg in segments)
                if (seg.color == color && seg.length > maxLen)
                    maxLen = seg.length;
            return maxLen == requiredCount;
        }
    }
}