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

        public string ToSyntax()
            => $"({FromNode.Name}, {Action.ToSyntax()}, {ToNode.Name})";

        public override string ToString()
        {
            return $"Edge with action type {this.Action.ToString()} from {FromNode} to {ToNode}";
        }

    }
}