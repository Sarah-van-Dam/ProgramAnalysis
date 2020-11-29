using System.Collections.Generic;
using System.Linq;
using Analyses.Algorithms;
using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Algorithms.ReversePostorder
{
    public class DepthFirstSpanningTreeTest 
    {
        [Fact]
        public void TestUsageOfNaturalOrderingAlgorithm()
        {
            var (programGraph, _) = ConstructTestProgramGraph();

            var analysis = new ReachingDefinitions(programGraph, WorklistImplementation.NaturalLoops);
            
            analysis.Analyse();

            Assert.Equal(14, analysis._worklistAlgorithm.BasicActionsNeeded);
        }

        /*

        [Fact]
        public void TestProgramWithNestedLoops()
        {
            var program = "int i; int j; i := 1; j := 0; while ( i < 5 ) { i := j +1 ; while ( j < 10 ) { j := j + 1; } j := 0; } j := 11;";
            var pg = new ProgramGraph(Parser.parse(program));
            
            var algorithm = new NaturalOrderingWorklist(pg);

            var stop = "stop";

        }
        
        */
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
                new Edge(nodes[ProgramGraph.StartNode], new IntDeclaration {VariableName = "x"}, nodes["q1"]),
                new Edge(nodes["q1"], new IntDeclaration{VariableName = "y"}, nodes["q2"]),
                new Edge(nodes["q2"],new IntAssignment{VariableName = "x", RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(1)}, nodes["q3"]),
                new Edge(nodes["q3"], new IntAssignment {VariableName = "y", RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(2)}, nodes["q4"]),
                new Edge(nodes["q4"],new Condition{Cond = MicroCTypes.booleanExpression.NewGreat(MicroCTypes.arithmeticExpression.NewVariable("y"),MicroCTypes.arithmeticExpression.NewVariable("x"))}, nodes["q5"]),
                new Edge(nodes["q4"],new Condition{Cond = MicroCTypes.booleanExpression.NewNot(MicroCTypes.booleanExpression.NewGreat(MicroCTypes.arithmeticExpression.NewVariable("y"),MicroCTypes.arithmeticExpression.NewVariable("x")))}, nodes["q6"]),
                new Edge(nodes["q5"], new IntAssignment{VariableName = "x", RightHandSide = MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewVariable("x"),MicroCTypes.arithmeticExpression.NewNumber(1))}, nodes["q7"]),
                new Edge(nodes["q7"], new IntAssignment{VariableName = "y", RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(0)}, nodes["q4"]),
                new Edge(nodes["q6"], new IntAssignment{VariableName = "x", RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(0)}, nodes[ProgramGraph.EndNode])
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