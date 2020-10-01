using System;
using Action = Analyses.Analysis.Actions;

namespace Analyses.Graph
{
    public class Edge
    {
        public Node FromNode { get; }
        public Node ToNode { get; }
        public Action Action { get; }

        public Edge(Node fromNode, Action action, Node toNode)
        {
            FromNode = fromNode;
            ToNode = toNode;
            Action = action;
        }

        public override string ToString()
            => $"({FromNode.Name}, {Action.ToSyntax()}, {ToNode.Name})";
    }
}