using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameCore
{
    public class ConditionDisplayUI : MonoBehaviour
    {
        public Canvas canvas;
        public GameObject textPrefab;
        public Camera cam;
        public Grid grid;
        public LevelConfig levelConfig;

        private List<GameObject> spawned = new List<GameObject>();

        void Start()
        {
            BuildDisplay();
        }

        void BuildDisplay()
        {
            ClearDisplay();
            GenerateRowLabels();
            GenerateColLabels();
        }

        void ClearDisplay()
        {
            foreach (var go in spawned)
                if (go != null) Destroy(go);
            spawned.Clear();
        }

        void GenerateRowLabels()
        {
            var conds = levelConfig.rowConditions;
            if (conds == null) return;
            int h = levelConfig.gridHeight;
            for (int y = 0; y < h; y++)
            {
                string text = y < conds.Length && conds[y] != null
                    ? FormatCondition(conds[y]) : "";
                if (string.IsNullOrEmpty(text)) continue;

                Vector3 worldPos = grid.GetCellCenterWorld(new Vector3Int(-1, y, 0));
                CreateLabel(text, worldPos);
            }
        }

        void GenerateColLabels()
        {
            var conds = levelConfig.colConditions;
            if (conds == null) return;
            int w = levelConfig.gridWidth;
            for (int x = 0; x < w; x++)
            {
                string text = x < conds.Length && conds[x] != null
                    ? FormatCondition(conds[x]) : "";
                if (string.IsNullOrEmpty(text)) continue;

                Vector3 worldPos = grid.GetCellCenterWorld(new Vector3Int(x, levelConfig.gridHeight, 0));
                CreateLabel(text, worldPos);
            }
        }

        void CreateLabel(string text, Vector3 worldPos)
        {
            if (textPrefab == null || canvas == null || cam == null) return;
            GameObject go = Instantiate(textPrefab, canvas.transform);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;

            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.position = screenPos;

            spawned.Add(go);
        }

        string FormatCondition(ColorCountGroup group)
        {
            var parts = new List<string>();
            foreach (var cc in group.entries)
                parts.Add($"{cc.color} {cc.count}");
            return string.Join("\n", parts);
        }
    }
}
