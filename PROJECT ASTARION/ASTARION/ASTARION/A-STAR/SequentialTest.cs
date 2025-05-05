using Raylib_cs;
using System.Collections.Generic;
using System.Diagnostics;

namespace ASTARION.A_STAR
{
    public class SequentialTest : TestCase
    {
        private int _currentTargetIndex;
        private int _currentStep;
        private float _timer;
        private List<Color> _targetColors = new List<Color>
        {
            Color.RED,
            Color.SKYBLUE,
            Color.GOLD
        };
        private readonly List<List<Node>> _completedPaths = new List<List<Node>>();

        public override void Initialize()
        {
            var sw = Stopwatch.StartNew();

            int[,] gridData = {
                {0,0,0,0,0,0,1,0,0,0},
                {0,1,1,0,1,1,0,1,1,0},
                {1,1,0,0,0,1,0,0,1,0},
                {0,0,0,1,0,0,0,0,0,0},  // <-- (3,3) is a 1 (blocked)
                {1,1,0,1,0,1,1,1,1,0},
                {0,1,0,1,0,0,0,1,0,0},
                {0,1,1,1,1,1,0,1,0,1},
                {0,0,0,0,0,0,0,1,0,1},
                {0,0,0,1,1,1,0,1,0,1},
                {0,0,0,0,0,0,0,0,0,0}
            };

            Grid = new Grid(gridData);
            Start = Grid.Nodes[0, 3];

            Targets = new List<Node>
            {
                Grid.Nodes[5, 3],  // was blocked
                Grid.Nodes[7, 1],
                Grid.Nodes[6, 8]
            };

            // ─── NEW: Force targets to be walkable ─────────────────────────────
            foreach (var t in Targets)
                t.Walkable = true;
            // ──────────────────────────────────────────────────────────────────

            _currentTargetIndex = 0;
            _currentStep = 0;
            _timer = 0;

            Path = CalculatePathSegment();

            sw.Stop();
            ExecutionTimeMs = sw.ElapsedMilliseconds;
            CalculateMetrics();
        }

        private List<Node> CalculatePathSegment()
        {
            // clear any old A* state
            foreach (var n in Grid.Nodes)
            {
                n.Parent = null;
                n.GCost = double.MaxValue;
                n.HCost = 0;
            }

            while (_currentTargetIndex < Targets.Count)
            {
                var origin = (_currentTargetIndex == 0) ? Start : Targets[_currentTargetIndex - 1];
                var dest = Targets[_currentTargetIndex];
                var path = Grid.FindPath(origin, dest);

                if (path != null && path.Count > 0)
                {
                    // ensure the first element is the origin
                    path.Insert(0, origin);
                    return path;
                }

                _currentTargetIndex++;
            }

            return null;
        }

        public override void Update()
        {
            if (Path == null) return;

            _timer += Raylib.GetFrameTime();
            if (_timer < 0.5f) return;

            _timer = 0;
            _currentStep++;

            if (_currentStep >= Path.Count)
            {
                _completedPaths.Add(new List<Node>(Path));
                _currentTargetIndex++;
                _currentStep = 0;
                Path = CalculatePathSegment();
            }
        }

        public override void Draw(int cellSize)
        {
            // Draw grid
            for (int x = 0; x < Grid.Width; x++)
                for (int y = 0; y < Grid.Height; y++)
                {
                    var n = Grid.Nodes[x, y];
                    var bg = n.Walkable ? Color.LIGHTGRAY : Color.DARKGRAY;
                    if (x == Start.X && y == Start.Y) bg = Color.GREEN;
                    Raylib.DrawRectangle(x * cellSize, y * cellSize,
                                         cellSize - 2, cellSize - 2, bg);
                }

            // Draw completed paths
            for (int i = 0; i < _completedPaths.Count && i < _targetColors.Count; i++)
                foreach (var n in _completedPaths[i])
                    Raylib.DrawRectangle(n.X * cellSize, n.Y * cellSize,
                                         cellSize - 2, cellSize - 2,
                                         _targetColors[i]);

            // Draw current in-progress path
            if (Path != null && _currentTargetIndex < _targetColors.Count)
                foreach (var n in Path)
                    Raylib.DrawRectangle(n.X * cellSize, n.Y * cellSize,
                                         cellSize - 2, cellSize - 2,
                                         _targetColors[_currentTargetIndex]);

            // Draw targets
            for (int i = 0; i < Targets.Count; i++)
            {
                var t = Targets[i];
                var c = _targetColors[i];
                Raylib.DrawCircle(
                    t.X * cellSize + cellSize / 2,
                    t.Y * cellSize + cellSize / 2,
                    cellSize / 3,
                    Raylib.ColorAlpha(c, 180f / 255f)
                );
                Raylib.DrawText((i + 1).ToString(),
                                t.X * cellSize + cellSize / 3,
                                t.Y * cellSize + cellSize / 3,
                                cellSize / 2, Color.BLACK);
            }

            // Draw agent
            if (Path != null && _currentStep < Path.Count)
            {
                var cur = Path[_currentStep];
                Raylib.DrawCircle(
                    cur.X * cellSize + cellSize / 2,
                    cur.Y * cellSize + cellSize / 2,
                    cellSize / 3, Color.ORANGE
                );
            }

            // Draw metrics
            Raylib.DrawRectangle(0, 0, 350, 130, new Color(255, 255, 255, 200));
            Raylib.DrawText($"Current Target: {_currentTargetIndex + 1}", 10, 10, 20, Color.BLACK);
            Raylib.DrawText($"Steps Taken:     {_currentStep}", 10, 40, 20, Color.BLACK);
            Raylib.DrawText($"Memory Usage:    {MemoryUsageMB}MB", 10, 70, 20, Color.BLACK);
            Raylib.DrawText($"Total Time:      {ExecutionTimeMs}ms", 10, 100, 20, Color.BLACK);
        }
    }
}
