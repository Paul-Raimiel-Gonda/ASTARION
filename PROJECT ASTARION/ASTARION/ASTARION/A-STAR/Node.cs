using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTARION.A_STAR
{
    public class Node : IComparable<Node>
    {
        public int X { get; }
        public int Y { get; }
        public bool Walkable { get; set; }
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
        public int CompareTo(Node other) => FCost.CompareTo(other.FCost) != 0 ?
            FCost.CompareTo(other.FCost) : HCost.CompareTo(other.HCost);
    }
}
