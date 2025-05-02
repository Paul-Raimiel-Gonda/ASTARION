using ASTARION;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ASTARION
{
    public class Node : IComparable<Node>
    {
        public int X { get; }
        public int Y { get; }
        public bool Walkable { get; }
        public double GCost { get; set; }
        public double HCost { get; set; }
        public double FCost => GCost + HCost;
        public Node Parent { get; set; }

        public Node(int x, int y, bool walkable)
        {
            X = x;
            Y = y;
            Walkable = walkable;
        }

        public override bool Equals(object obj) => obj is Node node && X == node.X && Y == node.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public int CompareTo(Node other) => FCost.CompareTo(other.FCost) != 0 ? FCost.CompareTo(other.FCost) : HCost.CompareTo(other.HCost);
    }

    public class Grid
    {
        public Node[,] Nodes { get; }
        public int Width { get; }
        public int Height { get; }

        public Grid(int[,] grid)
        {
            Width = grid.GetLength(0);
            Height = grid.GetLength(1);
            Nodes = new Node[Width, Height];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Nodes[x, y] = new Node(x, y, grid[x, y] == 0);
        }

        public List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int x = node.X + dx;
                    int y = node.Y + dy;

                    if (x < 0 || x >= Width || y < 0 || y >= Height) continue;

                    Node neighbor = Nodes[x, y];

                    if (!neighbor.Walkable) continue;

                    // Check diagonal movement validity
                    if (dx != 0 && dy != 0)
                    {
                        Node adj1 = Nodes[node.X + dx, node.Y];
                        Node adj2 = Nodes[node.X, node.Y + dy];
                        if (!adj1.Walkable || !adj2.Walkable) continue;
                    }

                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public List<Node> FindPath(Node start, Node target)
        {
            List<Node> openList = new List<Node> { start };
            HashSet<Node> closedList = new HashSet<Node>();

            start.GCost = 0;
            start.HCost = CalculateHeuristic(start, target);

            while (openList.Count > 0)
            {
                openList.Sort();
                Node current = openList[0];
                openList.RemoveAt(0);

                if (current.Equals(target))
                    return ReconstructPath(current);

                closedList.Add(current);

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (closedList.Contains(neighbor)) continue;

                    double tentativeGCost = current.GCost + CalculateDistance(current, neighbor);

                    if (!openList.Contains(neighbor) || tentativeGCost < neighbor.GCost)
                    {
                        neighbor.Parent = current;
                        neighbor.GCost = tentativeGCost;
                        neighbor.HCost = CalculateHeuristic(neighbor, target);

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private double CalculateHeuristic(Node a, Node b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return Math.Max(dx, dy);
        }

        private double CalculateDistance(Node a, Node b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return dx == 0 || dy == 0 ? 1 : Math.Sqrt(2);
        }

        private List<Node> ReconstructPath(Node node)
        {
            List<Node> path = new List<Node>();
            while (node != null)
            {
                path.Add(node);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        // New method to find all paths using DFS for verification
        public List<List<Node>> FindAllPaths(Node start, Node target)
        {
            List<List<Node>> allPaths = new List<List<Node>>();
            FindAllPathsDFS(start, target, new HashSet<Node>(), new List<Node>(), allPaths);
            return allPaths;
        }

        private void FindAllPathsDFS(Node current, Node target, HashSet<Node> visited,
                                   List<Node> currentPath, List<List<Node>> allPaths)
        {
            if (!current.Walkable || visited.Contains(current)) return;

            visited.Add(current);
            currentPath.Add(current);

            if (current.Equals(target))
            {
                allPaths.Add(new List<Node>(currentPath));
            }
            else
            {
                foreach (Node neighbor in GetNeighbors(current))
                {
                    FindAllPathsDFS(neighbor, target, new HashSet<Node>(visited),
                                  new List<Node>(currentPath), allPaths);
                }
            }
        }

        // New verification method
        public bool VerifyOptimalPath(List<Node> aStarPath)
        {
            if (aStarPath == null) return true;

            var allPaths = FindAllPaths(aStarPath.First(), aStarPath.Last());
            if (allPaths.Count == 0) return true;

            int shortestLength = allPaths.Min(p => p.Count);
            return aStarPath.Count == shortestLength;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Test grid with multiple possible paths
            int[,] gridData = {
                {0,0,0,0,1,0,1,1,1,1},
                {0,1,0,0,1,0,1,0,1,0},
                {1,1,1,0,0,0,1,0,0,0},
                {0,1,0,1,1,0,1,0,1,0},
                {0,1,0,1,0,0,1,0,1,0},
                {0,0,0,1,0,0,1,0,1,0},
                {1,1,0,0,0,1,0,0,1,0},
                {0,1,0,1,1,0,0,1,0,0},
                {0,1,0,1,0,0,0,1,0,1},
                {0,0,0,0,0,1,1,1,0,0}
            };

            Grid grid = new Grid(gridData);
            Node start = grid.Nodes[0, 1];
            Node target = grid.Nodes[9, 9];

            // Find paths using both algorithms
            List<Node> aStarPath = grid.FindPath(start, target);
            List<List<Node>> allPaths = grid.FindAllPaths(start, target);
            bool isOptimal = grid.VerifyOptimalPath(aStarPath);

            const int cellSize = 100;
            Raylib.InitWindow(1000, 1000, "A* Pathfinding Verification");
            Raylib.SetTargetFPS(60);

            // Animation and drawing variables
            int currentStep = 0;
            float timer = 0;
            bool showAllPaths = false;

            while (!Raylib.WindowShouldClose())
            {
                // Toggle view with SPACE
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                    showAllPaths = !showAllPaths;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);

                // Draw grid
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        Color color = grid.Nodes[x, y].Walkable ? Color.LIGHTGRAY : Color.DARKGRAY;
                        if (x == start.X && y == start.Y) color = Color.GREEN;
                        else if (x == target.X && y == target.Y) color = Color.RED;
                        Raylib.DrawRectangle(x * cellSize, y * cellSize, cellSize - 2, cellSize - 2, color);
                    }
                }

                

                // Draw A* path
                if (aStarPath != null)
                {
                    foreach (Node node in aStarPath)
                    {
                        if (node.Equals(start) || node.Equals(target)) continue;
                        Raylib.DrawRectangle(node.X * cellSize, node.Y * cellSize,
                                           cellSize - 2, cellSize - 2, Color.BLUE);
                    }
                }

                // Draw all paths if toggled
                if (showAllPaths && allPaths != null)
                {
                    foreach (var path in allPaths)
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
                                new Color(255, 165, 0, 128)  // Semi-transparent blue
                            );
                        }
                    }
                }

                // Animate NPC
                if (aStarPath != null && currentStep < aStarPath.Count)
                {
                    Node current = aStarPath[currentStep];
                    Raylib.DrawCircle(
                        current.X * cellSize + cellSize / 2,
                        current.Y * cellSize + cellSize / 2,
                        cellSize / 3,
                        Color.ORANGE
                    );

                    timer += Raylib.GetFrameTime();
                    if (timer >= 0.5f)
                    {
                        currentStep++;
                        timer = 0;
                    }
                }

                // Draw info panel
                Raylib.DrawRectangle(0, 0, 200, 80, new Color(255, 255, 255, 200));
                Raylib.DrawText($"A* Steps: {(aStarPath?.Count - 1) ?? 0}", 10, 10, 20, Color.BLACK);
                Raylib.DrawText($"Total Paths: {allPaths?.Count ?? 0}", 10, 35, 20, Color.BLACK);
                Raylib.DrawText($"Optimal: {(isOptimal ? "YES" : "NO")}", 10, 60, 20,
                              isOptimal ? Color.GREEN : Color.RED);

                Raylib.DrawText("Press SPACE to toggle", 300, 10, 20, Color.BLACK);
                Raylib.DrawText("all paths view", 300, 35, 20, Color.BLACK);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}