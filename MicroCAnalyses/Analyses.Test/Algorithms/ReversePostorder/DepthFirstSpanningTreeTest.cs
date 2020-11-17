using System.Collections.Generic;
using System.Linq;
using Analyses.Algorithms.ReversePostorder;
using Analyses.Analysis.Actions;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Algorithms.ReversePostorder
{
    public class DepthFirstSpanningTreeTest 
    {

        [Fact]
        public void TestCreationOfDepthFirstSpanningTree()
        {
            var (programGraph, edges) = ConstructTestProgramGraph();
            var expectedEdges = new List<Edge>
            {
                edges[0], edges[1], edges[2], edges[3], edges[4], edges[6], edges[5], edges[8]
            };
            
            
            var spanningTree = DepthFirstSpanningTree.CreateDepthFirstSpanningTreeFromProgramGraph(programGraph);
            
            Assert.Equal(expectedEdges, spanningTree.Edges);
        }


        private static (ProgramGraph programGraph, List<Edge> edgesUsedInPg) ConstructTestProgramGraph()
        {
            var variableNames = new HashSet<string>{"x", "y"};
            var nodes = new Dictionary<string, Node>
            {
                {ProgramGraph.StartNode, new Node(ProgramGraph.StartNode)},
                {"q1", new Node("q1")},
                {"q2", new Node("q2")},
                {"q3", new Node("q3")},
                {"q4", new Node("q4")},
                {"q5", new Node("q5")},
                {"q6", new Node("q6")},
                {"q7", new Node("q7")},
                {ProgramGraph.EndNode, new Node(ProgramGraph.EndNode)}
            };
            
            var edges = new List<Edge>
            {
                new Edge(nodes[ProgramGraph.StartNode], new IntDeclaration(), nodes["q1"]),
                new Edge(nodes["q1"], new IntDeclaration(), nodes["q2"]),
                new Edge(nodes["q2"],new IntAssignment(), nodes["q3"]),
                new Edge(nodes["q3"], new IntAssignment(), nodes["q4"]),
                new Edge(nodes["q4"],new Condition(), nodes["q5"]),
                new Edge(nodes["q4"],new Condition(), nodes["q6"]),
                new Edge(nodes["q5"], new IntAssignment(), nodes["q7"]),
                new Edge(nodes["q7"], new IntAssignment(), nodes["q4"]),
                new Edge(nodes["q6"], new IntAssignment(), nodes[ProgramGraph.EndNode])
            };

            nodes[ProgramGraph.StartNode].OutGoingEdges.Add(edges[0]);
            nodes["q1"].InGoingEdges.Add(edges[0]);
            nodes["q1"].OutGoingEdges.Add(edges[1]);
            nodes["q2"].InGoingEdges.Add(edges[1]);
            nodes["q2"].OutGoingEdges.Add(edges[2]);
            nodes["q3"].InGoingEdges.Add(edges[2]);
            nodes["q3"].OutGoingEdges.Add(edges[3]);
            nodes["q4"].InGoingEdges.Add(edges[3]);
            nodes["q4"].InGoingEdges.Add(edges[7]);
            nodes["q4"].OutGoingEdges.Add(edges[4]);
            nodes["q4"].OutGoingEdges.Add(edges[5]);
            nodes["q5"].InGoingEdges.Add(edges[4]);
            nodes["q5"].OutGoingEdges.Add(edges[6]);
            nodes["q6"].InGoingEdges.Add(edges[5]);
            nodes["q6"].OutGoingEdges.Add(edges[8]);
            nodes["q7"].InGoingEdges.Add(edges[6]);
            nodes["q7"].OutGoingEdges.Add(edges[7]);
            nodes[ProgramGraph.EndNode].InGoingEdges.Add(edges[8]);

            return (new ProgramGraph(variableNames,nodes.Values.ToHashSet(), edges.ToHashSet()), edges);
        }
    }
}