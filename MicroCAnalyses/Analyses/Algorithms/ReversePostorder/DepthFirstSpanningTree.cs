using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;
using Analyses.Helpers;

namespace Analyses.Algorithms.ReversePostorder
{
    public class DepthFirstSpanningTree
    {
        public List<Edge> Edges { get; }

        private DepthFirstSpanningTree(List<Edge> edges)
        {
            Edges = edges;
        }
        
        public static DepthFirstSpanningTree CreateDepthFirstSpanningTreeFromProgramGraph(ProgramGraph programGraph)
        {
            var nodes = programGraph.Nodes.ToList();
            SortingHelper.OrderNodesByDirection(nodes, true);
            var startingNode = nodes.First();
            var visitedNodes = new HashSet<Node>(){startingNode};
            
            var edgesForTree = DFS(startingNode.OutGoingEdges, visitedNodes);
            
            return new DepthFirstSpanningTree(edgesForTree);
        }

        private static List<Edge> DFS(HashSet<Edge> edgesOfNode, HashSet<Node> visitedNodes)
        {
            var edgesNeeded = new List<Edge>();
            var currentNodeEdges = edgesOfNode.ToList();
            SortingHelper.OrderEdgesByDirection(currentNodeEdges,true);
            foreach (var edge in currentNodeEdges.Where(edge => !visitedNodes.Contains(edge.ToNode)))
            {
                edgesNeeded.Add(edge);
                visitedNodes.Add(edge.ToNode);
                edgesNeeded.AddRange(DFS(edge.ToNode.OutGoingEdges, visitedNodes));
            }

            return edgesNeeded;
        }
    }
}