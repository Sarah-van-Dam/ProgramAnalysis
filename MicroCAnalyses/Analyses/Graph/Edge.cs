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
            return $"Edge with action type {Action} from {FromNode} to {ToNode}";
        }

        public override int GetHashCode()
        {
            return FromNode.GetHashCode() ^ Action.GetHashCode() ^ ToNode.GetHashCode();
        }

        
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Edge other))
            {
                return false;
            }

            return FromNode == other.FromNode && Action == other.Action && ToNode == other.ToNode;
        }
    }
}