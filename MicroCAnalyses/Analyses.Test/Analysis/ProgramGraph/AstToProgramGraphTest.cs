using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Analysis.ProgramGraph
{
    public class AstToProgramGraphTest
    {
        public AstToProgramGraphTest() {}

        [Fact]
        public void TestSimpleGraph()
        {
            Graph.ProgramGraph graph = new Graph.ProgramGraph(
                Parser.parse(
                    "int x;"
                )
            );

            Assert.True(graph.Nodes.Count == 2, "The number of nodes in the graph should be 2, got " + graph.Nodes.Count);
            Assert.True(graph.Edges.Any(edge => edge.Action is IntDeclaration), "Expected an edge with a variable declaration, but none was found.");
        }

        [Fact]
        public void TestGenerateFibonnachiGraph()
        {
            Graph.ProgramGraph graph = new Graph.ProgramGraph(
                Parser.parse(
                    "int f2; int input; int current; f1 := 1; f2 := 1; read input; if (input == 0 | input == 1 ){ current := 1; } while (input > 1) { current := f1 + f2; f2 := f1; f1 := current; input := input - 1; } write current;"
                )
            );

            Assert.True(graph.Nodes.Count == 16, "The number of nodes in the graph should be 16, got " + graph.Nodes.Count);
            Assert.True(graph.Nodes.Any(node => node.Name == "Q14"), "Expected Node 'Q14' was not found.");
            Assert.True(graph.Nodes.Any(node => node.Name == "Q_End"), "Expected Node 'Q_End' was not found.");
            Assert.True(graph.Edges.Count == 17, "The number of edges in the graph should be 17, got " + graph.Edges.Count);
            Assert.True(graph.Edges.Any(edge => edge.Action is Condition), "Expected an edge with a condition, but none was found.");
        }

        [Fact]
        public void TestGenerateSumIntegerGraph()
        {
            Graph.ProgramGraph graph = new Graph.ProgramGraph(
                Parser.parse(
                    "int n[6]; int x; int r; n[0] := 2; n[1] := 7; n[2] := 1; n[3] := 9; n[4] := 2; n[5] := 5; x := 0; r := 0; while (x < 6) { r := r + n[x]; x := x + 1; }"
                )
            );

            Assert.True(graph.Nodes.Count == 16, "The number of nodes in the graph should be 16, got " + graph.Nodes.Count);
            Assert.True(graph.Nodes.Any(node => node.Name == "Q9"), "Expected Node 'Q9' was not found.");
            Assert.True(graph.Nodes.Any(node => node.Name == "Q_End"), "Expected Node 'Q_End' was not found.");
            Assert.True(graph.Edges.Count == 15, "The number of edges in the graph should be 15, got " + graph.Edges.Count);
            Assert.True(graph.Edges.Any(edge => edge.Action is ArrayAssignment), "Expected an edge with an array assignment, but none was found.");
        }
    }
}