using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;

namespace Analyses.Helpers
{
    public static class EdgesHelper
    {
        public static List<Edge> OrderEdgesByDirection(ProgramGraph program, bool isForward)
        {
            var edgesList = new List<Edge>();
            var nodeList = program.Nodes.ToList();
            nodeList.Sort((first, second) =>
            {
                if (first.Name == ProgramGraph.EndNode)
                {
                    return int.MaxValue;
                }

                return first.Index - second.Index;
            });
            foreach (var node in nodeList)
            {
                var edges = program.Edges.Where(e => e.FromNode.Equals(node)).ToList();
                edges.Sort((first, second) =>
                {
                    if (second.ToNode.Name == ProgramGraph.EndNode)
                    {
                        return int.MaxValue;
                    }

                    return first.ToNode.Index - second.ToNode.Index;
                });
                edgesList.AddRange(edges);
            }

            if (!isForward)
            {
                edgesList.Reverse();
            }

            return edgesList;
        }
        
    }
}