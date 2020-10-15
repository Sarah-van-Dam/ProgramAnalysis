using System.Collections.Generic;
using System.Linq;

namespace Analyses.Graph
{
    public class Node
    {
        public int Index { get; }
        public string Name { get; }
        public HashSet<Edge> InGoingEdges { get; }
        public HashSet<Edge> OutGoingEdges { get; }

        public Node(string name)
        {
            Name = name;
            Index = ParseNameToIndex();
            InGoingEdges = new HashSet<Edge>();
            OutGoingEdges = new HashSet<Edge>();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Node))
            {
                return false;
            }

            var other = obj as Node;
            return Name == other.Name;
        }

        private int ParseNameToIndex()
        {
            if (Name == "q_start")
            {
                return 0;
            }
            if (Name == "q_end")
            {
                return -1;
            }
            return int.Parse(Name.Where(char.IsDigit).ToArray());
        }

        public override string ToString()
        {
            return $"Node {Name}";
        }
    }
}