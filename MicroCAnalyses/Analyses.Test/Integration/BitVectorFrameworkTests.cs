using System.Collections.Generic;
using Analyses.Analysis.BitVector.AvailableExpressionAnalysis;
using Analyses.Analysis.BitVector.LiveVariablesAnalysis;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Integration
{
    public class BitVectorFrameworkTests
    {
        private const string Factorial =
            "int y; int x; y :=1; x :=5; while (x > 1) { y := x * y; x := x - 1; } x := 0;";

        private const string Fibonacci = "int f1; int f2; int input; int current; f1:=1; f2:=1; read input; " +
                                         "if( input == 0 | input == 1) { current := 1; } while ( input > 1) " +
                                         "{ current := f1 + f2; f2:=f1; f1:= current; input := input - 1; } write current;";

        /// <summary>
        /// Test E2E RD analysis with a hardcoded expect
        /// </summary>
        [Fact]
        public void ReachingDefinitionsTest()
        {
            //Arrange
            var expectedResult = new Dictionary<Node, ReachingDefinitionConstraints>()
            {
                {
                    new Node("q_start"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "?", "q_start")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "?", "q_start")}
                                }
                            }
                    }
                },
                {
                    new Node("q1"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "?", "q_start")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "?", "q_start")}
                                }
                            }
                    }
                },
                {
                    new Node("q2"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "?", "q_start")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "?", "q_start")}
                                }
                            }
                    }
                },
                {
                    new Node("q3"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)> {("y", "q2", "q3")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "?", "q_start")}
                                }
                            }
                    }
                },
                {
                    new Node("q4"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "q2", "q3"), ("y", "q5", "q7")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "q3", "q4"), ("x", "q7", "q4")}
                                }
                            }
                    }
                },
                {
                    new Node("q5"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "q2", "q3"), ("y", "q5", "q7")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "q3", "q4"), ("x", "q7", "q4")}
                                }
                            }
                    }
                },
                {
                    new Node("q6"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "q2", "q3"), ("y", "q5", "q7")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "q3", "q4"), ("x", "q7", "q4")}
                                }
                            }
                    }
                },
                {
                    new Node("q7"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)> {("y", "q5", "q7")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "q3", "q4"), ("x", "q7", "q4")}
                                }
                            }
                    }
                },
                {
                    new Node("q_end"),
                    new ReachingDefinitionConstraints()
                    {
                        VariableToPossibleAssignments =
                            new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                            {
                                {
                                    "y",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("y", "q2", "q3"), ("y", "q5", "q7")}
                                },
                                {
                                    "x",
                                    new HashSet<(string variable, string startNode, string endNode)>
                                        {("x", "q6", "q_end")}
                                }
                            }
                    }
                },
            };

            var ast = Parser.parse(Factorial);
            var pg = new ProgramGraph(ast);
            var rd = new ReachingDefinitions(pg);

            //Act
            rd.Analyse();

            //Assert
            Assert.Equal(expectedResult.Keys, rd.FinalConstraintsForNodes.Keys);
            foreach (var (key, expectedConstraint) in expectedResult)
            {
                Assert.Equal(expectedConstraint, rd.FinalConstraintsForNodes[key]);
            }
        }

        [Fact]
        public void LiveVariablesTest()
        {
            var expectedResult = new Dictionary<Node, LiveVariableConstraint>()
            {
                {
                    new Node("q_start"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() { }
                    }
                },
                {
                    new Node("q1"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() { }
                    }
                },
                {
                    new Node("q2"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() { }
                    }
                },
                {
                    new Node("q3"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() { }
                    }
                },
                {
                    new Node("q4"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current"}
                    }
                },
                {
                    new Node("q5"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "f1"}
                    }
                },
                {
                    new Node("q6"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "f1", "f2"}
                    }
                },
                {
                    new Node("q7"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "f1", "f2", "input"}
                    }
                },
                {
                    new Node("q8"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"f1", "f2", "input"}
                    }
                },
                {
                    new Node("q9"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "f1", "f2", "input"}
                    }
                },
                {
                    new Node("q10"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"f1", "f2", "input"}
                    }
                },
                {
                    new Node("q11"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current"}
                    }
                },
                {
                    new Node("q12"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "input", "f1"}
                    }
                },
                {
                    new Node("q13"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "input", "f2"}
                    }
                },
                {
                    new Node("q14"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() {"current", "f1", "f2", "input"}
                    }
                },
                {
                    new Node("q_end"),
                    new LiveVariableConstraint()
                    {
                        LiveVariables = new HashSet<string>() { }
                    }
                },
            };

            var ast = Parser.parse(Fibonacci);
            var pg = new ProgramGraph(ast);
            var live = new LiveVariables(pg);

            //Act
            live.Analyse();

            //Assert
            Assert.Equal(expectedResult.Keys, live.FinalConstraintsForNodes.Keys);
            foreach (var (key, expectedConstraint) in expectedResult)
            {
                Assert.Equal(expectedConstraint, live.FinalConstraintsForNodes[key]);
            }
        }

        [Fact]
        public void TestAvailableExpressionsAnalysis()
        {
            var expectedResult = new Dictionary<Node, AvailableExpressionConstraints>()
            {
                {
                    new Node("q_start"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        )
                },
                {
                    new Node("q1"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q2"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q3"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q4"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q5"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q6"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q7"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q8"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewOr(
                                MicroCTypes.booleanExpression.NewEqual(
                                    MicroCTypes.arithmeticExpression.NewVariable("input"),
                                    MicroCTypes.arithmeticExpression.NewNumber(0)
                                    ),MicroCTypes.booleanExpression.NewEqual(
                                    MicroCTypes.arithmeticExpression.NewVariable("input"),
                                    MicroCTypes.arithmeticExpression.NewNumber(1)
                                )
                                ),
                            MicroCTypes.booleanExpression.NewEqual(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(0)
                            ),
                            MicroCTypes.booleanExpression.NewEqual(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            )
                        }
                    )
                },
                {
                    new Node("q9"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                    )
                },
                {
                    new Node("q10"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                                )
                        }
                    )
                },
                {
                    new Node("q11"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            ),
                            MicroCTypes.booleanExpression.NewNot(MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            ))
                        }
                    )
                },
                {
                    new Node("q12"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>()
                        {
                            MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewVariable("f1"),
                                MicroCTypes.arithmeticExpression.NewVariable("f2"))
                        },
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            )
                        }
                    )
                },
                {
                    new Node("q13"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            )
                        }
                    )
                },
                {
                    new Node("q14"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            )
                        }
                    )
                },
                {
                    new Node("q_end"),
                    new AvailableExpressionConstraints(
                        new HashSet<MicroCTypes.arithmeticExpression>(),
                        new HashSet<MicroCTypes.booleanExpression>()
                        {
                            MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            ),
                            MicroCTypes.booleanExpression.NewNot(MicroCTypes.booleanExpression.NewGreat(
                                MicroCTypes.arithmeticExpression.NewVariable("input"),
                                MicroCTypes.arithmeticExpression.NewNumber(1)
                            ))
                        }
                    )
                },
            };

            var ast = Parser.parse(Fibonacci);
            var pg = new ProgramGraph(ast);
            var availableExpressions = new AvailableExpressions(pg);

            //Act
            availableExpressions.Analyse();

            //Assert
            Assert.Equal(expectedResult.Keys, availableExpressions.FinalConstraintsForNodes.Keys);
            foreach (var (key, expectedConstraint) in expectedResult)
            {
                Assert.Equal(expectedConstraint, availableExpressions.FinalConstraintsForNodes[key]);
            }
        }
    }
}