using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            Index = this.ParseNameToIndex();
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
            if (this.Name == "q_start")
            {
                return 0;
            }
            if (this.Name == "q_end")
            {
                return 999; //TODO: fix me - this will break for larger programs
            }
            return int.Parse(this.Name.Where(char.IsDigit).ToArray());
        }

        public override string ToString()
        {
            return $"Node {this.Name}";
        }
    }
}