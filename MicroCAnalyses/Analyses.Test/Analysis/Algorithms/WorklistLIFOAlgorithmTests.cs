using Analyses.Algorithms.Stack;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Analysis.Monotone.DetectionOfSignsAnalysis;
using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Analyses.Test.Analysis.Algorithms
{
	public class WorklistLIFOAlgorithmTests
	{

        private ReachingDefinitions _analysis;
        private readonly ProgramGraph _graphFibonacci;
        private readonly ProgramGraph _graphAddIntegers;

        public WorklistLIFOAlgorithmTests()
        {
            _graphFibonacci = new ProgramGraph(
                Parser.parse(
                    "int f2; int input; int current; f1 := 1; f2 := 1; read input; if (input == 0 | input == 1 ){ current := 1; } " +
                    "while (input > 1) { current := f1 + f2; f2 := f1; f1 := current; input := input - 1; } write current;"
                )
            );
            _graphAddIntegers = new ProgramGraph(
                Parser.parse(
                    "int a; int b; int c; a := 3; read b; c := a + b; write c;"
                )
            );
        }
        
        [Fact]
        public void TestSameInsertCountFibonacci()
        {

            _analysis = new ReachingDefinitions(_graphFibonacci, Analyses.Algorithms.WorklistImplementation.Lifo);
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var algo = _analysis._worklistAlgorithm as WorklistLIFOAlgorithm;
            var counter1 = algo.Counter;

            _analysis = new ReachingDefinitions(_graphFibonacci, Analyses.Algorithms.WorklistImplementation.Lifo);
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            algo = _analysis._worklistAlgorithm as WorklistLIFOAlgorithm;
            var counter2 = algo.Counter;

            Assert.True(counter1 > 0, $"Expected Insert() count to be positive for counter1, as program is not empty.");
            Assert.True(counter2 > 0, $"Expected Insert() count to be positive for counter2, as program is not empty.");
            Assert.True(counter1 == counter2, $"Expected Insert() count to be identical for both executions of the program. Got ({counter1}, {counter2})");

            Assert.True(counter1 == 70, $"Expected Insert() count for counter1 to be 70. This is based on observing a previous run. Got {counter1}");
        }
        
        [Fact]
        public void TestSameInsertCountSumIntArray()
        {

            _analysis = new ReachingDefinitions(_graphAddIntegers, Analyses.Algorithms.WorklistImplementation.Lifo);
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            var algo = _analysis._worklistAlgorithm as WorklistLIFOAlgorithm;
            var counter1 = algo.Counter;

            _analysis = new ReachingDefinitions(_graphAddIntegers, Analyses.Algorithms.WorklistImplementation.Lifo);
            _analysis.InitializeConstraints();
            _analysis.Analyse();

            algo = _analysis._worklistAlgorithm as WorklistLIFOAlgorithm;
            var counter2 = algo.Counter;

            Assert.True(counter1 > 0, $"Expected Insert() count to be positive for counter1, as program is not empty.");
            Assert.True(counter2 > 0, $"Expected Insert() count to be positive for counter2, as program is not empty.");
            Assert.True(counter1 == counter2, $"Expected Insert() count to be identical for both executions of the program. Got ({counter1}, {counter2})");

            Assert.True(counter1 == 22, $"Expected Insert() count for counter1 to be 22. This is based on observing a previous run. Got {counter1}");
        }

    }
}
