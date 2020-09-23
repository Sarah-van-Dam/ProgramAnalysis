using System;

namespace Analyses
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
    }
}