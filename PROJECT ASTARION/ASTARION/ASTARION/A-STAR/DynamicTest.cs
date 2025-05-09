using System;
using Raylib_cs;
using System.Diagnostics;
using System.Collections.Generic;
using System.Numerics;

namespace ASTARION.A_STAR
{
    public class DynamicTest : TestCase
    {
        private Node _entityPos;
        private Vector2 _npcPos;
        private int _changeCounter;

        // Overlay dimensions
        private const int OverlayWidth = 350;
        private const int OverlayHeight = 130;

        public override void Initialize()
        {
            var sw = Stopwatch.StartNew();

            int[,] gridData = {
                {0,1,0,0,1,0},
                {0,0,0,0,0,0},
                {1,0,0,0,1,0},
                {0,0,1,0,0,0},
                {1,0,0,0,1,0},
                {0,0,1,0,0,0}
            };

            Grid = new Grid(gridData);
            Start = Grid.Nodes[0, 0];
            Targets = new List<Node> { Grid.Nodes[5, 5] };

            _npcPos = new Vector2(0, 3);
            Grid.Nodes[(int)_npcPos.X, (int)_npcPos.Y].Walkable = false;

            _entityPos = Start;

            Path = Grid.FindPath(_entityPos, Targets[0]);
            Path?.TrimExcess();

            sw.Stop();
            ExecutionTimeMs = sw.ElapsedMilliseconds;
            _changeCounter = 0;
        }

        public override void Update()
        {
            if (!Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                return;

            if (_entityPos.Equals(Targets[0]))
                return;

            // Reset Parent pointers to avoid stale chains
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    Grid.Nodes[x, y].Parent = null;
                }
            }

            // Move NPC and ensure revert on dead-end
            var oldNpc = _npcPos;
            Grid.Nodes[(int)_npcPos.X, (int)_npcPos.Y].Walkable = true;
            _npcPos.X = (_npcPos.X + 1) % Grid.Width;
            Grid.Nodes[(int)_npcPos.X, (int)_npcPos.Y].Walkable = false;

            var newPath = Grid.FindPath(_entityPos, Targets[0]);
            if (newPath == null || newPath.Count <= 1)
            {
                // revert NPC if no progress
                Grid.Nodes[(int)_npcPos.X, (int)_npcPos.Y].Walkable = true;
                _npcPos = oldNpc;
                Grid.Nodes[(int)_npcPos.X, (int)_npcPos.Y].Walkable = false;
                return;
            }

            Path = newPath;
            Path.TrimExcess();

            _entityPos = Path[1];
            _changeCounter++;
        }

        public override void Draw(int cellSize)
        {
            for (int x = 0; x < Grid.Width; x++)
                for (int y = 0; y < Grid.Height; y++)
                {
                    Color color = Grid.Nodes[x, y].Walkable ? Color.LIGHTGRAY : Color.DARKGRAY;
                    if (x == Start.X && y == Start.Y)
                        color = Color.GREEN;
                    else if (x == Targets[0].X && y == Targets[0].Y)
                        color = Color.PURPLE;
                    Raylib.DrawRectangle(x * cellSize, y * cellSize, cellSize - 2, cellSize - 2, color);
                }

            if (Path != null)
                for (int i = 1; i < Path.Count - 1; i++)
                {
                    var node = Path[i];
                    Raylib.DrawRectangle(node.X * cellSize, node.Y * cellSize, cellSize - 2, cellSize - 2, Color.BLUE);
                }

            Raylib.DrawRectangle((int)_npcPos.X * cellSize, (int)_npcPos.Y * cellSize, cellSize - 2, cellSize - 2, Color.RED);
            Raylib.DrawCircle(_entityPos.X * cellSize + cellSize / 2, _entityPos.Y * cellSize + cellSize / 2, cellSize / 3, Color.ORANGE);

            int overlayX = Raylib.GetScreenWidth() - OverlayWidth - 10;
            int overlayY = Raylib.GetScreenHeight() - OverlayHeight - 10;
            Raylib.DrawRectangle(overlayX, overlayY, OverlayWidth, OverlayHeight, new Color(255, 255, 255, 220));
            Raylib.DrawText("Press SPACE to step", overlayX + 10, overlayY + 10, 20, Color.BLACK);
            Raylib.DrawText($"Moves: {_changeCounter}", overlayX + 10, overlayY + 40, 20, Color.BLACK);
            Raylib.DrawText($"Path Length: {Path?.Count ?? 0}", overlayX + 10, overlayY + 70, 20, Color.BLACK);
            Raylib.DrawText($"NPC Pos: ({(int)_npcPos.X},{(int)_npcPos.Y})", overlayX + 10, overlayY + 100, 20, Color.BLACK);
        }
    }
}
