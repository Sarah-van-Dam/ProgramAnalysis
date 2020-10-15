using System.Collections.Generic;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Integration
{
    public class BitVectorFrameworkTests
    {
        private const string Factorial = "int y; int x; y :=1; x :=5; while (x > 1) { y := x * y; x := x - 1; } x := 0;";
        
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
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "?", "q_start")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "?", "q_start")}}
                        } }
                    
                },
                {
                    new Node("q1"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "?", "q_start")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "?", "q_start")}}
                        } }
                },
                {
                    new Node("q2"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "?", "q_start")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "?", "q_start")}}
                        } }
                },
                {
                    new Node("q3"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "q2", "q3")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "?", "q_start")}}
                        } }
                },
                {
                    new Node("q4"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "q2", "q3"), ("y","q5","q7")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "q3", "q4"), ("x","q7","q4")}}
                        } }
                    
                },
                {
                    new Node("q5"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "q2", "q3"), ("y","q5","q7")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "q3", "q4"), ("x","q7","q4")}}
                        } }
                },
                {
                    new Node("q6"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "q2", "q3"), ("y","q5","q7")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "q3", "q4"), ("x","q7","q4")}}
                        } }
                },
                {
                    new Node("q7"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{ ("y","q5","q7")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{("x", "q3", "q4"), ("x","q7","q4")}}
                        } }
                },
                {
                    new Node("q_end"),
                    new ReachingDefinitionConstraints() { VariableToPossibleAssignments =
                        new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
                        {
                            {"y", new HashSet<(string variable, string startNode, string endNode)>{("y", "q2", "q3"), ("y","q5","q7")} },
                            {"x", new HashSet<(string variable, string startNode, string endNode)>{ ("x","q6","q_end")}}
                        } }
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
                Assert.Equal(expectedConstraint,rd.FinalConstraintsForNodes[key] );
            }
        }
    }
}