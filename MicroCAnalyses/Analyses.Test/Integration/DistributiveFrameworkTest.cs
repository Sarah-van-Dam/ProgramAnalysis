using System.Collections.Generic;
using Analyses.Analysis.BitVector.LiveVariablesAnalysis;
using Analyses.Analysis.Monotone;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Integration
{
    public class DistributiveFrameworkTest
    {
        private const string Fibonacci = "int f1; int f2; int input; int current; f1:=1; f2:=1; read input; " +
                                         "if( input == 0 | input == 1) { current := 1; } while ( input > 1) " +
                                         "{ current := f1 + f2; f2:=f1; f1:= current; input := input - 1; } write current;";

        [Fact]
        public void TestFaintVariablesAnalysis()
        {
            var expectedResult = new Dictionary<Node, FaintVariableConstraint>()
            {
                {
                    new Node("q_start"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { }
                    }
                },
                {
                    new Node("q1"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { }
                    }
                },
                {
                    new Node("q2"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { }
                    } 
                },
                {
                    new Node("q3"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { }
                    } 
                },
                {
                    new Node("q4"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> {"current" }
                    } 
                },
                {
                    new Node("q5"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> {"current", "f1" }
                    } 
                },
                {
                    new Node("q6"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> {"current", "f1", "f2" }
                    } 
                },
                {
                    new Node("q7"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> {"current", "f1", "f2", "input" }
                    } 
                },
                {
                    new Node("q8"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { "f1", "f2", "input" }
                    } 
                },
                {
                    new Node("q9"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> {"current", "f1", "f2", "input" }
                    } 
                },
                {
                    new Node("q10"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { "f1", "f2", "input" }
                    } 
                },
                {
                    new Node("q11"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> {"current" }
                    } 
                },
                {
                    new Node("q12"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { "f1", "current", "input" }
                    } 
                },
                {
                    new Node("q13"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { "current", "f2", "input" }
                    } 
                },
                {
                    new Node("q14"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { "current", "f1", "f2", "input" }
                    } 
                },
                {
                    new Node("q_end"),
                    new FaintVariableConstraint() 
                    {
                        StronglyLivedVariables = new HashSet<string> { }
                    } 
                },
            };
            
            var ast = Parser.parse(Fibonacci);
            var pg = new ProgramGraph(ast);
            var live = new FaintVariables(pg);
            
            //Act
            live.Analyse();

            //Assert
            Assert.Equal(expectedResult.Keys, live.FinalConstraintsForNodes.Keys);
            foreach (var (key, expectedConstraint) in expectedResult)
            {
                Assert.Equal(expectedConstraint,live.FinalConstraintsForNodes[key]);
            }
        }

        [Fact]
        public void TestDangerousVariablesAnalysis()
        {
            var expectedResult = new Dictionary<Node, DangerousVariablesConstraint>()
            {
                {
                    new Node("q_start"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "f1", "f2", "input" }
                    }
                },
                {
                    new Node("q1"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "f1", "f2", "input" }
                    }
                },
                {
                    new Node("q2"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "f1", "f2", "input" }
                    }
                },
                {
                    new Node("q3"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "f1", "f2", "input" }
                    }
                },
                {
                    new Node("q4"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> {"current", "f1", "f2", "input" }
                    }
                },
                {
                    new Node("q5"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input", "f2" }
                    }
                },
                {
                    new Node("q6"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input" }
                    }
                },
                {
                    new Node("q7"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input" }
                    }
                },
                {
                    new Node("q8"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input" }
                    }
                },
                {
                    new Node("q9"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input" }
                    }
                },
                {
                    new Node("q10"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> {  "current", "input" }
                    }
                },
                {
                    new Node("q11"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input" }
                    }
                },
                {
                    new Node("q12"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "input" }
                    }
                },
                {
                    new Node("q13"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "input" }
                    }
                },
                {
                    new Node("q14"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "input" }
                    }
                },
                {
                    new Node("q_end"),
                    new DangerousVariablesConstraint()
                    {
                        DangerousVariables = new HashSet<string> { "current", "input" }
                    }
                },
            };

            var ast = Parser.parse(Fibonacci);
            var pg = new ProgramGraph(ast);
            var live = new DangerousVariables(pg);

            //Act
            live.Analyse();

            //Assert
            Assert.Equal(expectedResult.Keys, live.FinalConstraintsForNodes.Keys);
            foreach (var (key, expectedConstraint) in expectedResult)
            {
                Assert.Equal(expectedConstraint, live.FinalConstraintsForNodes[key]);
            }
        }
    }
}