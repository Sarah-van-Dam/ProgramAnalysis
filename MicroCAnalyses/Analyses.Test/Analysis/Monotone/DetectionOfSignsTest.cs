using System.Collections.Generic;
using System.Linq;
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

            _analysis = new DetectionOfSigns(_graph, new HashSet<(string variable, Sign)>() { ("f2", Sign.Positive), ("f1", Sign.Positive), ("input", Sign.Zero), ("current", Sign.Zero) });
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var endNode = _graph.Nodes.First(node => node.Name == ProgramGraph.EndNode);
            var constraint = _analysis.FinalConstraintsForNodes[endNode] as DetectionOfSignsConstraint;

            // Simple variable count check
            Assert.True(constraint.VariableSigns.Count == 4, $"Expected 4 variables in resulting constraint, only given {constraint.VariableSigns.Count}.");
            
            // Check existence of variables in output
            Assert.True(constraint.VariableSigns.ContainsKey("f1"), "Variable 'f1' was not found in the resulting constraint.");
            Assert.True(constraint.VariableSigns.ContainsKey("f2"), "Variable 'f2' was not found in the resulting constraint.");
            Assert.True(constraint.VariableSigns.ContainsKey("input"), "Variable 'input' was not found in the resulting constraint.");
            Assert.True(constraint.VariableSigns.ContainsKey("current"), "Variable 'current' was not found in the resulting constraint.");

            // Check expected signs for each variable
            Assert.True(constraint.VariableSigns["f1"].signs.SetEquals(new HashSet<Sign>() { Sign.Positive }),
                $"Expected 'f1' to be {{ Positive }}, but got {{ {string.Join(", ", constraint.VariableSigns["f1"].signs)} }}.");
            Assert.True(constraint.VariableSigns["f2"].signs.SetEquals(new HashSet<Sign>() { Sign.Positive }),
                $"Expected 'f2' to be {{ Positive }}, but got {{ {string.Join(", ", constraint.VariableSigns["f2"].signs)} }}.");
            Assert.True(constraint.VariableSigns["input"].signs.SetEquals(new HashSet<Sign>() { Sign.Negative, Sign.Zero, Sign.Positive }),
                $"Expected 'input' to be {{ Negative, Zero, Positive }}, but got {{ {string.Join(", ", constraint.VariableSigns["input"].signs)} }}.");
            Assert.True(constraint.VariableSigns["current"].signs.SetEquals(new HashSet<Sign>() { Sign.Zero, Sign.Positive }),
                $"Expected 'current' to be {{ Zero, Positive }}, but got {{ {string.Join(", ", constraint.VariableSigns["current"].signs)} }}.");
        }

        [Fact]
        public void TestSupportMultipleInputSigns()
        {

            _analysis = new DetectionOfSigns(_graph, new HashSet<(string variable, Sign)>() { ("f2", Sign.Positive), ("f2", Sign.Negative), ("f1", Sign.Positive), ("input", Sign.Zero), ("current", Sign.Zero) });
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var startNode = _graph.Nodes.First(node => node.Name == ProgramGraph.StartNode);
            var constraintStart = _analysis.FinalConstraintsForNodes[startNode] as DetectionOfSignsConstraint;

            var endNode = _graph.Nodes.First(node => node.Name == ProgramGraph.EndNode);
            var constraintEnd = _analysis.FinalConstraintsForNodes[endNode] as DetectionOfSignsConstraint;

            // f2 can only be positive, as it is assigned during the program.
            // for the entry node, q_start, f2 should be { Negative, Positive }.
            Assert.True(constraintEnd.VariableSigns["f2"].signs.SetEquals(new HashSet<Sign>() { Sign.Positive }),
                $"Expected 'f2' to be {{ Positive }} for {ProgramGraph.EndNode}, but got {{ {string.Join(", ", constraintEnd.VariableSigns["f2"].signs)} }}.");

            Assert.True(constraintStart.VariableSigns["f2"].signs.SetEquals(new HashSet<Sign>() { Sign.Negative, Sign.Positive }),
                $"Expected 'f2' to be {{ Negative, Positive }} for {ProgramGraph.StartNode}, but got {{ {string.Join(", ", constraintStart.VariableSigns["f2"].signs)} }}.");
        }

        [Fact]
        public void TestSameInputSameOutput()
        {

            _analysis = new DetectionOfSigns(_graph, new HashSet<(string variable, Sign)>() { ("f2", Sign.Negative), ("f1", Sign.Positive), ("input", Sign.Positive), ("current", Sign.Zero) });
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var endNode1 = _graph.Nodes.First(node => node.Name == ProgramGraph.EndNode);
            var constraint1 = _analysis.FinalConstraintsForNodes[endNode1] as DetectionOfSignsConstraint;

            _analysis = new DetectionOfSigns(_graph, new HashSet<(string variable, Sign)>() { ("f2", Sign.Negative), ("f1", Sign.Positive), ("input", Sign.Positive), ("current", Sign.Zero) });
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var endNode2 = _graph.Nodes.First(node => node.Name == ProgramGraph.EndNode);
            var constraint2 = _analysis.FinalConstraintsForNodes[endNode2] as DetectionOfSignsConstraint;

            Assert.True(constraint1.VariableSigns.Count == constraint2.VariableSigns.Count,
                $"Expected resulting constraints to have same length (4), but got ({constraint1.VariableSigns.Count}, {constraint2.VariableSigns.Count})");

            Assert.True(constraint1.Equals(constraint2),
                $"Expected resulting constraints to be equal, but got:\nConstraint 1: {constraint1}\nConstraint 2: {constraint2}");

        }

    }
}