using System;
using System.Collections.Generic;

namespace Analyses
{
    public class Node
    {
        public string Name { get; }
        public HashSet<Edge> InGoingEdges { get; }
        public HashSet<Edge> OutGoingEdges { get; }

        public Node(string name)
        {
            Name = name;
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
    }
}