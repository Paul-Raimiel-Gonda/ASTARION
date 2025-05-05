using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTARION.A_STAR
{
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
            visited.Remove(current);
            currentPath.RemoveAt(currentPath.Count - 1);
        }

        public bool VerifyOptimalPath(List<Node> aStarPath)
        {
            if (aStarPath == null) return true;

            var allPaths = FindAllPaths(aStarPath.First(), aStarPath.Last());
            if (allPaths.Count == 0) return true;

            int shortestLength = allPaths.Min(p => p.Count);
            return aStarPath.Count == shortestLength;
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
    }
}
