using Raylib_cs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace ASTARION.A_STAR
{
    public class ShortestPathTest : TestCase
    {
        private int _currentStep;
        private float _timer;
        private bool _showAllPaths;
        private Node _target;
        private List<List<Node>> _allPaths;

        public override void Initialize()
        {
            var sw = Stopwatch.StartNew();

            int[,] gridData = {
                {0,0,0,0,1,0,1,1,1,1},
                {0,1,0,0,1,0,1,0,1,0},
                {1,1,1,0,0,0,1,0,0,0},
                {0,1,0,1,1,0,1,0,1,0},
                {0,1,0,1,0,0,1,0,1,0},
                {0,0,0,1,0,0,1,0,0,0},
                {1,1,0,0,0,1,0,0,1,0},
                {0,1,0,1,1,1,0,1,0,0},
                {0,1,0,1,0,0,0,1,0,1},
                {0,0,0,0,0,1,1,1,0,0}
            };

            Grid = new Grid(gridData);
            Start = Grid.Nodes[0, 2];
            _target = Grid.Nodes[9, 9];

            Path = Grid.FindPath(Start, _target);
            _allPaths = Grid.FindAllPaths(Start, _target);
            IsOptimal = Grid.VerifyOptimalPath(Path);

            sw.Stop();
            ExecutionTimeMs = sw.ElapsedMilliseconds;
            CalculateMetrics();

            _currentStep = 0;
            _timer = 0;
            _showAllPaths = false;
        }

        public override void Update()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                _showAllPaths = !_showAllPaths;

            if (Path == null || _currentStep >= Path.Count) return;

            _timer += Raylib.GetFrameTime();
            if (_timer >= 0.5f)
            {
                _currentStep++;
                _timer = 0;
            }
        }

        public override void Draw(int cellSize)
        {
            // Draw grid
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    Color color = Grid.Nodes[x, y].Walkable ?
                        Color.LIGHTGRAY : Color.DARKGRAY;

                    if (x == Start.X && y == Start.Y) color = Color.GREEN;
                    else if (x == _target.X && y == _target.Y) color = Color.RED;

                    Raylib.DrawRectangle(x * cellSize, y * cellSize,
                        cellSize - 2, cellSize - 2, color);
                }
            }

            // Draw A* path
            if (Path != null)
            {
                foreach (Node node in Path)
                {
                    if (node.Equals(Start) || node.Equals(_target)) continue;
                    Raylib.DrawRectangle(node.X * cellSize, node.Y * cellSize,
                        cellSize - 2, cellSize - 2, Color.BLUE);
                }
            }

            // Draw all paths
            if (_showAllPaths && _allPaths != null)
            {
                foreach (var path in _allPaths)
                {
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        Raylib.DrawLineEx(
                            new Vector2(
                                path[i].X * cellSize + cellSize / 2,
                                path[i].Y * cellSize + cellSize / 2),
                            new Vector2(
                                path[i + 1].X * cellSize + cellSize / 2,
                                path[i + 1].Y * cellSize + cellSize / 2),
                            2,
                            new Color(255, 165, 0, 50)
                        );
                    }
                }
            }

            // Draw entity
            if (Path != null && _currentStep < Path.Count)
            {
                Node current = Path[_currentStep];
                Raylib.DrawCircle(
                    current.X * cellSize + cellSize / 2,
                    current.Y * cellSize + cellSize / 2,
                    cellSize / 3,
                    Color.ORANGE
                );
            }

            // Draw metrics
            Raylib.DrawRectangle(0, 0, 350, 160, new Color(255, 255, 255, 200));
            Raylib.DrawText($"A* Time: {ExecutionTimeMs}ms", 10, 10, 20, Color.BLACK);
            Raylib.DrawText($"A* Steps: {(Path?.Count - 1) ?? 0}", 10, 40, 20, Color.BLACK);
            Raylib.DrawText($"Total Paths: {_allPaths?.Count ?? 0}", 10, 70, 20, Color.BLACK);
            Raylib.DrawText($"Memory Usage: {MemoryUsageMB}MB", 10, 100, 20, Color.BLACK);
            Raylib.DrawText($"Optimal: {(IsOptimal ? "YES" : "NO")}", 10, 130, 20,
                IsOptimal ? Color.GREEN : Color.RED);
        }

        protected void CalculateMetrics()
        {
            var process = Process.GetCurrentProcess();
            MemoryUsageMB = process.WorkingSet64 / (1024 * 1024); // Convert bytes to MB
        }
    }
}