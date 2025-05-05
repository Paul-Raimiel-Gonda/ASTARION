using System;
using Raylib_cs;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ASTARION.A_STAR
{
    public class DynamicTest : TestCase
    {
        private Random _random = new Random();
        private List<Vector2> _modifiedCells = new List<Vector2>();
        private int _changeCounter;
        private float _timer;
        private int _currentStep;

        public override void Initialize()
        {
            var sw = Stopwatch.StartNew();

            int[,] gridData = {
                {0,0,0,0,0,0},
                {0,0,0,0,0,0},
                {0,0,0,0,0,0},
                {0,0,0,0,0,0},
                {0,0,0,0,0,0},
                {0,0,0,0,0,0}
            };

            Grid = new Grid(gridData);
            Start = Grid.Nodes[0, 0];
            Targets = new List<Node> { Grid.Nodes[4, 4] };

            Path = Grid.FindPath(Start, Targets[0]);

            sw.Stop();
            ExecutionTimeMs = sw.ElapsedMilliseconds;
            CalculateMetrics();

            _changeCounter = 0;
            _timer = 0;
            _currentStep = 0;
        }

        public override void Update()
        {
            if (Path == null || _changeCounter >= 100) return;

            _timer += Raylib.GetFrameTime();
            if (_timer >= 0.3f)
            {
                ModifyGrid();
                RecalculatePath();
                _changeCounter++;
                _timer = 0;
            }
        }

        private void ModifyGrid()
        {
            for (int i = 0; i < 3; i++)
            {
                int x = _random.Next(Grid.Width);
                int y = _random.Next(Grid.Height);

                if (Grid.Nodes[x, y] == Start || Grid.Nodes[x, y] == Targets[0])
                    continue;

                Grid.Nodes[x, y].Walkable = !Grid.Nodes[x, y].Walkable;
                _modifiedCells.Add(new Vector2(x, y));
            }
        }

        private void RecalculatePath()
        {
            Node currentPosition = Path != null && _currentStep < Path.Count ?
                Path[_currentStep] :
                Start;

            Path = Grid.FindPath(currentPosition, Targets[0]);
            _currentStep = 0;
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
                    else if (x == Targets[0].X && y == Targets[0].Y) color = Color.RED;

                    Raylib.DrawRectangle(x * cellSize, y * cellSize,
                        cellSize - 2, cellSize - 2, color);
                }
            }

            // Draw modified cells
            foreach (Vector2 cell in _modifiedCells)
            {
                Raylib.DrawRectangle(
                    (int)cell.X * cellSize,
                    (int)cell.Y * cellSize,
                    cellSize - 2,
                    cellSize - 2,
                    new Color(255, 0, 0, 128)
                );
            }

            // Draw path
            if (Path != null)
            {
                foreach (Node node in Path)
                {
                    if (node.Equals(Start) || node.Equals(Targets[0])) continue;
                    Raylib.DrawRectangle(node.X * cellSize, node.Y * cellSize,
                        cellSize - 2, cellSize - 2, Color.BLUE);
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
                _currentStep++;
            }

            // Draw metrics
            Raylib.DrawRectangle(0, 0, 300, 100, new Color(255, 255, 255, 200));
            Raylib.DrawText($"Changes: {_changeCounter}", 10, 10, 20, Color.BLACK);
            Raylib.DrawText($"Path Length: {Path?.Count ?? 0}", 10, 40, 20, Color.BLACK);
            Raylib.DrawText($"Time: {ExecutionTimeMs}ms", 10, 70, 20, Color.BLACK);
        }
    }
}
