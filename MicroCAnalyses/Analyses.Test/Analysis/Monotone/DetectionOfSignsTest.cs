using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis.Actions;
using Analyses.Analysis.Monotone.DetectionOfSignsAnalysis;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Analysis.Monotone
{
    public class DetectionOfSignsTest
    {
        private DetectionOfSigns _analysis;
        private readonly ProgramGraph _graph;

        public DetectionOfSignsTest()
        {
            _graph = new ProgramGraph(
                Parser.parse(
                    "int f2; int input; int current; f1 := 1; f2 := 1; read input; if (input == 0 | input == 1 ){ current := 1; } " +
                    "while (input > 1) { current := f1 + f2; f2 := f1; f1 := current; input := input - 1; } write current;"
                )
            );
        }

        [Fact]
        public void TestFinalSignsCorrectness()
        {

            _analysis = new DetectionOfSigns(_graph, new HashSet<(string variable, Sign)>() { ("f2", Sign.Positive), ("f1", Sign.Positive) });
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var endNode = _graph.Nodes.First(node => node.Name == ProgramGraph.EndNode);
            var constraint = _analysis.FinalConstraintsForNodes[endNode] as DetectionOfSignsConstraint;

            System.Diagnostics.Debug.Print(constraint.ToString());
        }

    }
}