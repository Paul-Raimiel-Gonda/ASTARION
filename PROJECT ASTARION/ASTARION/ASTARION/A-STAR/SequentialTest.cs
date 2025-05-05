using System;
using Raylib_cs;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTARION.A_STAR
{
    public class SequentialTest : TestCase
    {
        private int _currentTargetIndex;
        private int _currentStep;
        private float _timer;
        private List<List<Node>> _paths = new List<List<Node>>();
        private List<Color> _pathColors = new List<Color>
        {
            Color.BLUE,
            Color.PURPLE,
            Color.DARKPURPLE
        };

        public override void Initialize()
        {
            var sw = Stopwatch.StartNew();

            int[,] gridData = {
                {0,0,0,0,0,0,0,0,0,0},
                {0,1,1,0,1,1,0,1,1,0},
                {0,1,0,0,0,1,0,0,1,0},
                {0,0,0,1,0,0,1,0,0,0},
                {0,1,0,1,0,1,1,1,0,0},
                {0,1,0,0,0,0,0,1,0,0},
                {0,1,1,1,1,1,0,1,0,0},
                {0,0,0,0,0,0,0,1,0,0},
                {0,1,1,1,1,1,1,1,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            };

            Grid = new Grid(gridData);
            Start = Grid.Nodes[0, 0];
            Targets = new List<Node> {
                Grid.Nodes[3, 3],
                Grid.Nodes[6, 6],
                Grid.Nodes[9, 9]
            };

            _currentTargetIndex = 0;
            CalculateNextPath();

            sw.Stop();
            ExecutionTimeMs = sw.ElapsedMilliseconds;
            CalculateMetrics();

            _currentStep = 0;
            _timer = 0;
        }

        private void CalculateNextPath()
        {
            if (_currentTargetIndex >= Targets.Count) return;

            Node currentStart = _currentTargetIndex == 0 ?
                Start :
                Targets[_currentTargetIndex - 1];

            var path = Grid.FindPath(currentStart, Targets[_currentTargetIndex]);
            if (path != null)
            {
                _paths.Add(path);
                Path = path;
                _currentStep = 0;
            }
        }

        public override void Update()
        {
            if (Path == null || _currentStep >= Path.Count)
            {
                if (_currentTargetIndex < Targets.Count - 1)
                {
                    _currentTargetIndex++;
                    CalculateNextPath();
                }
                return;
            }

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
                    else if (Targets.Contains(Grid.Nodes[x, y]))
                        color = Color.RED;

                    Raylib.DrawRectangle(x * cellSize, y * cellSize,
                        cellSize - 2, cellSize - 2, color);
                }
            }

            // Draw all paths
            for (int i = 0; i < _paths.Count; i++)
            {
                if (i >= _pathColors.Count) break;
                DrawPathSegment(_paths[i], _pathColors[i], cellSize);
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
            Raylib.DrawRectangle(0, 0, 300, 100, new Color(255, 255, 255, 200));
            Raylib.DrawText($"Current Target: {_currentTargetIndex + 1}", 10, 10, 20, Color.BLACK);
            Raylib.DrawText($"Total Steps: {_paths.Sum(p => p?.Count ?? 0)}", 10, 40, 20, Color.BLACK);
            Raylib.DrawText($"Time: {ExecutionTimeMs}ms", 10, 70, 20, Color.BLACK);
        }

        private void DrawPathSegment(List<Node> path, Color color, int cellSize)
        {
            if (path == null) return;

            foreach (Node node in path)
            {
                if (node.Equals(Start)) continue;
                Raylib.DrawRectangle(node.X * cellSize, node.Y * cellSize,
                    cellSize - 2, cellSize - 2, color);
            }
        }
    }
}
