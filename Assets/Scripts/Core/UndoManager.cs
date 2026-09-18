using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class GameState
    {
        public Vector2Int playerPos;
        public int[,] colorGrid;
        public List<BoxState> boxStates = new List<BoxState>();

        public GameState Clone()
        {
            var clone = new GameState { playerPos = playerPos };
            int w = colorGrid.GetLength(0), h = colorGrid.GetLength(1);
            clone.colorGrid = new int[w, h];
            System.Array.Copy(colorGrid, clone.colorGrid, w * h);
            foreach (var bs in boxStates)
                clone.boxStates.Add(bs.Clone());
            return clone;
        }
    }

    public class BoxState
    {
        public Box boxRef;
        public BoxColor color;
        public Vector2Int gridPos;
        public int remainingPushes;
        public bool alive;

        public BoxState Clone()
        {
            return new BoxState
            {
                boxRef = boxRef,
                color = color,
                gridPos = gridPos,
                remainingPushes = remainingPushes,
                alive = alive,
            };
        }
    }

    public class UndoManager
    {
        private Stack<GameState> history = new Stack<GameState>();

        public bool CanUndo => history.Count > 0;

        public void Record(GameState state)
        {
            history.Push(state.Clone());
        }

        public GameState Pop()
        {
            return history.Count > 0 ? history.Pop() : null;
        }

        public void Clear()
        {
            history.Clear();
        }
    }
}
