using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;

namespace Analyses.Helpers
{
    public static class SortingHelper
    {
        public static void OrderNodesByDirection(List<Node> nodes, bool isForward)
        {
            nodes.Sort((first, second) =>
            {
                if (first.Name == ProgramGraph.EndNode)
                {
                    return int.MaxValue;
                }

                return first.Index - second.Index;
            });

            if (!isForward)
            {
                nodes.Reverse();
            }
        }

        public static void OrderEdgesByDirection(List<Edge> edges, bool isForward)
        {
            edges.Sort((first, second) =>
            {
                if (second.ToNode.Name == ProgramGraph.EndNode)
                {
                    return int.MaxValue;
                }

                return first.ToNode.Index - second.ToNode.Index;
            });

            if (!isForward)
            {
                edges.Reverse();
            }
        }
    }
}