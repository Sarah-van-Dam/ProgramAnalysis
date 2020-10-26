using System.Collections.Generic;
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
        
    }
}