using System;
using System.Collections.Generic;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Analyses.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace Analyses.Test.Integration
{
    public class BitVectorFrameworkTests
    {
        private readonly String factorial =
            "int y; int x; y :=1; x :=5; while (x > 1) { y := x * y; x := x - 1; } x := 0;";


        /// <summary>
        /// Test E2E RD analysis with a hardcoded expect
        /// </summary>
        [Fact]
        public void ReachingDefinitionsTest()
        {
            //Arrange
            var expectedResult = new Dictionary<Node, AnalysisResult<(string, string, string)>>()
            {
                {
                    new Node("q_start"),
                    new AnalysisResult<(string, string, string)> { ("y", "?", "q_start"), ("x", "?", "q_start") }
                },
                {
                    new Node("q2"),
                    new AnalysisResult<(string, string, string)> { ("y", "?", "q_start"), ("x", "?", "q_start") }
                },
                {
                    new Node("q1"),
                    new AnalysisResult<(string, string, string)> { ("y", "?", "q_start"), ("x", "?", "q_start") }
                },
                {
                    new Node("q5"),
                    new AnalysisResult<(string, string, string)> { ("y", "q1", "q5"), ("x", "?", "q_start") }
                },
                {
                    new Node("q4"),
                    new AnalysisResult<(string, string, string)>
                    {
                        ("y", "q1", "q5"), ("x", "q5", "q4"), ("y", "q6", "q7"), ("x", "q7", "q4")
                    }
                },
                {
                    new Node("q6"),
                    new AnalysisResult<(string, string, string)>
                    {
                        ("y", "q1", "q5"), ("x", "q5", "q4"), ("y", "q6", "q7"), ("x", "q7", "q4")
                    }
                },
                {
                    new Node("q7"),
                    new AnalysisResult<(string, string, string)>
                    {
                        ("y", "q1", "q5"), ("x", "q7", "q4"), ("y", "q6", "q7")
                    }
                },
                {
                    new Node("q3"),
                    new AnalysisResult<(string, string, string)>
                    {
                        ("y", "q1", "q5"), ("x", "q7", "q4"), ("y", "q6", "q7"), ("x", "q5", "q4")
                    }
                },
                {
                    new Node("q_end"),
                    new AnalysisResult<(string, string, string)>
                    {
                        ("y", "q1", "q5"), ("x", "q7", "q4"), ("y", "q6", "q7")
                    }
                },
            };

            var ast = Parser.parse(factorial);
            var pg = new ProgramGraph(ast);
            var rd = new ReachingDefinitions(pg);
            
            //Act
            rd.Analyse();

            //Assert

            var result = rd.AnalysisResult;
            Assert.AreEqual(expectedResult, result);
        }
    }
}