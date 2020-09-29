using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Xunit;

namespace Analyses.Test.Analysis.BitVector
{
    public class ReachingDefinitionsTest
    {
        private ReachingDefinitions _analysis;
        private const string SingleVariableName = "x";
        
        public ReachingDefinitionsTest()
        {
            
        }

        [Theory]
        [MemberData(nameof(EmptyKillSetActions))]
        public void TestKillOnEmptyKillSetDoesNotOverrideDefaultSettings(string variableName, Edge edge)
        {
            var variableNames = new HashSet<string> {variableName};
            if (edge.Action is RecordDeclaration)
            {
                variableNames = new HashSet<string>{$"{variableName}.{RecordMember.Fst}", $"{variableName}.{RecordMember.Snd}"};
            }
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(variableNames));

            //If it is record declaration there will be two results, otherwise one.
            var originalSize = _analysis.Constraints.VariableToPossibleAssignments.Count;
            var defaultConstraints = new HashSet<(string,string,string)>();
            if (originalSize == 1)
            {
                defaultConstraints = _analysis.Constraints.VariableToPossibleAssignments.Single().Value;
            } else if (originalSize == 2)
            {
                defaultConstraints.UnionWith(_analysis.Constraints.VariableToPossibleAssignments
                    .Single(s => s.Key == $"{variableName}.{RecordMember.Fst}").Value);
                defaultConstraints.UnionWith(_analysis.Constraints.VariableToPossibleAssignments
                    .Single(s => s.Key == $"{variableName}.{RecordMember.Snd}").Value);
            }
            else
            {
                throw new Exception("Original size was not as expected");
            }
            
            _analysis.Kill(edge);

            Assert.Equal(originalSize,_analysis.Constraints.VariableToPossibleAssignments.Count);
            if (originalSize == 1)
            {
                Assert.Equal(defaultConstraints, _analysis.Constraints.VariableToPossibleAssignments.Single().Value);
            }
            else
            {
                var constraints =
                    _analysis.Constraints.VariableToPossibleAssignments
                        .Single(s => s.Key == $"{variableName}.{RecordMember.Fst}").Value
                        .Union(_analysis.Constraints.VariableToPossibleAssignments
                            .Single(s => s.Key == $"{variableName}.{RecordMember.Snd}").Value)
                        .ToHashSet();
                Assert.Equal(defaultConstraints,constraints);
            }
        }

        [Fact]
        public void TestKillOnVariableAssignmentKillsDefaultConstraint()
        {
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string>{"x"}));    
            
            _analysis.Kill(new Edge(null, new IntAssignment{VariableName = "x",RightHandSide = "2+1"}, null));

            var (key, value) = Assert.Single(_analysis.Constraints.VariableToPossibleAssignments);
            Assert.Empty(value);
        }

        [Fact]
        public void TestKillOnRecordMemberAssignmentKillsDefaultConstraintOfMember()
        {
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string>{"R.Fst", "R.Snd"}));
            var originalSndConstraint =
                _analysis.Constraints.VariableToPossibleAssignments.Single(v => v.Key == "R.Snd").Value;

            _analysis.Kill(new Edge(null, new RecordMemberAssignment{RecordName = "R", RecordMember = RecordMember.Fst,RightHandSide = "2+1"}, null));

            var (key, value) = Assert.Single(_analysis.Constraints.VariableToPossibleAssignments.Where( v => v.Key == "R.Fst"));
            Assert.Empty(value);
            Assert.Equal(originalSndConstraint, _analysis.Constraints.VariableToPossibleAssignments.Single(v => v.Key == "R.Snd").Value);
        }
        
        [Fact]
        public void TestKillOnRecordAssignmentKillsDefaultConstraints()
        {
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string>{"R.Fst", "R.Snd"}));    
            
            _analysis.Kill(new Edge(null, new RecordAssignment{RecordName = "R",FirstExpression = "2+1", SecondExpression = "1+2"}, null));

            Assert.Equal(2, _analysis.Constraints.VariableToPossibleAssignments.Count);
            Assert.Empty(_analysis.Constraints.VariableToPossibleAssignments["R.Fst"]);
            Assert.Empty(_analysis.Constraints.VariableToPossibleAssignments["R.Snd"]);
        }

        [Fact]
        public void TestGenerateOnAssignmentsAddVariableNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            var (fromNode, toNode) = InitializeAnalysis(fromNodeName, toNodeName, SingleVariableName);
            var edge = new Edge(fromNode, new IntAssignment {VariableName = SingleVariableName, RightHandSide = "2"},
                toNode);
            _analysis.Kill(edge);
            
            _analysis.Generate(edge);

            var constraint = Assert.Single(_analysis.Constraints.VariableToPossibleAssignments).Value;
            var insertedTuple = constraint.Single();
            Assert.Equal(fromNodeName, insertedTuple.startNode);
            Assert.Equal(toNodeName, insertedTuple.endNode);
            Assert.Equal(SingleVariableName, insertedTuple.variable);
        }
        
        [Fact]
        public void TestGenerateOnArrayAssignmentsAddVariableNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            const string arrayName = "A";
            var (fromNode, toNode) = InitializeAnalysis(fromNodeName, toNodeName, arrayName);
            var edge = new Edge(fromNode, new ArrayAssignment {ArrayName = arrayName, Index = "1",  RightHandSide = "2"},
                toNode);
            
            _analysis.Generate(edge);

            var constraint = Assert.Single(_analysis.Constraints.VariableToPossibleAssignments).Value;
            var insertedTuple = constraint.Single();
            Assert.Equal(fromNodeName, insertedTuple.startNode);
            Assert.Equal(toNodeName, insertedTuple.endNode);
            Assert.Equal(arrayName, insertedTuple.variable);
        }
        
        [Fact]
        public void TestGenerateOnRecordAssignmentsAddVariableNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            const string recordName = "R";
            var recordFirst = $"{recordName}.{RecordMember.Fst}";
            var recordSecond = $"{recordName}.{RecordMember.Snd}";
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {recordFirst, recordSecond}));
            var fromNode = new Node(fromNodeName);
            var toNode = new Node(toNodeName);
            var edge = new Edge(fromNode, new RecordAssignment {RecordName = recordName, FirstExpression = "1",  SecondExpression = "2"},
                toNode);
            _analysis.Kill(edge);
            
            _analysis.Generate(edge);

            Assert.Equal(2, _analysis.Constraints.VariableToPossibleAssignments.Count);
            var firstConstraint =
                Assert.Single(
                    _analysis.Constraints.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordFirst)).Value;
            var firstTuple = firstConstraint.Single();
            Assert.Equal(fromNodeName, firstTuple.startNode);
            Assert.Equal(toNodeName, firstTuple.endNode);
            Assert.Equal(recordFirst, firstTuple.variable);
            
            var secondConstraint =
                Assert.Single(
                    _analysis.Constraints.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordSecond)).Value;
            var secondTuple = secondConstraint.Single();
            Assert.Equal(fromNodeName, secondTuple.startNode);
            Assert.Equal(toNodeName, secondTuple.endNode);
            Assert.Equal(recordSecond, secondTuple.variable);
        }
        
        [Fact]
        public void TestGenerateOnRecordMemberAssignmentsAddVariableNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            const string recordName = "R";
            var recordFirst = $"{recordName}.{RecordMember.Fst}";
            var recordSecond = $"{recordName}.{RecordMember.Snd}";
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {recordFirst, recordSecond}));
            var fromNode = new Node(fromNodeName);
            var toNode = new Node(toNodeName);
            var edge = new Edge(fromNode, new RecordMemberAssignment {RecordName = recordName, RecordMember = RecordMember.Fst,  RightHandSide = "1"},
                toNode);
            _analysis.Kill(edge);
            
            _analysis.Generate(edge);

            Assert.Equal(2, _analysis.Constraints.VariableToPossibleAssignments.Count);
            var firstConstraint =
                Assert.Single(
                    _analysis.Constraints.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordFirst)).Value;
            var firstTuple = firstConstraint.Single();
            Assert.Equal(fromNodeName, firstTuple.startNode);
            Assert.Equal(toNodeName, firstTuple.endNode);
            Assert.Equal(recordFirst, firstTuple.variable);
            
            var secondConstraint =
                Assert.Single(
                    _analysis.Constraints.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordSecond)).Value;
            var secondTuple = secondConstraint.Single();
            Assert.Equal("?", secondTuple.startNode);
            Assert.Equal(ProgramGraph.StartNode, secondTuple.endNode);
            Assert.Equal(recordSecond, secondTuple.variable);
        }
        
        [Fact]
        public void TestGenerateOnReadRecordMemberAddVariableNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            const string recordName = "R";
            var recordFirst = $"{recordName}.{RecordMember.Fst}";
            var recordSecond = $"{recordName}.{RecordMember.Snd}";
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {recordFirst, recordSecond}));
            var fromNode = new Node(fromNodeName);
            var toNode = new Node(toNodeName);
            var edge = new Edge(fromNode, new ReadRecordMember {RecordName = recordName, RecordMember = RecordMember.Fst},
                toNode);
            _analysis.Kill(edge);
            
            _analysis.Generate(edge);

            Assert.Equal(2, _analysis.Constraints.VariableToPossibleAssignments.Count);
            var firstConstraint =
                Assert.Single(
                    _analysis.Constraints.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordFirst)).Value;
            var firstTuple = firstConstraint.Single();
            Assert.Equal(fromNodeName, firstTuple.startNode);
            Assert.Equal(toNodeName, firstTuple.endNode);
            Assert.Equal(recordFirst, firstTuple.variable);
            
            var secondConstraint =
                Assert.Single(
                    _analysis.Constraints.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordSecond)).Value;
            var secondTuple = secondConstraint.Single();
            Assert.Equal("?", secondTuple.startNode);
            Assert.Equal(ProgramGraph.StartNode, secondTuple.endNode);
            Assert.Equal(recordSecond, secondTuple.variable);
        }
        
        [Fact]
        public void TestGenerateOnReadVariableAddsVariableNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            var (fromNode, toNode) = InitializeAnalysis(fromNodeName, toNodeName, SingleVariableName);
            var edge = new Edge(fromNode, new ReadVariable {VariableName = SingleVariableName},
                toNode);
            _analysis.Kill(edge);
            
            _analysis.Generate(edge);

            var constraint = Assert.Single(_analysis.Constraints.VariableToPossibleAssignments).Value;
            var insertedTuple = constraint.Single();
            Assert.Equal(fromNodeName, insertedTuple.startNode);
            Assert.Equal(toNodeName, insertedTuple.endNode);
            Assert.Equal(SingleVariableName, insertedTuple.variable);
        }
        
        [Fact]
        public void TestGenerateOnReadArrayIndexAddsArrayNameAndNodesToTuple()
        {
            const string fromNodeName = "q1";
            const string toNodeName = "q2";
            const string arrayName = "A";
            var (fromNode, toNode) = InitializeAnalysis(fromNodeName, toNodeName, arrayName);
            var edge = new Edge(fromNode, new ArrayAssignment {ArrayName = arrayName, Index = "1",  RightHandSide = "2"},
                toNode);
            
            _analysis.Generate(edge);

            var constraint = Assert.Single(_analysis.Constraints.VariableToPossibleAssignments).Value;
            var insertedTuple = constraint.Single();
            Assert.Equal(fromNodeName, insertedTuple.startNode);
            Assert.Equal(toNodeName, insertedTuple.endNode);
            Assert.Equal(arrayName, insertedTuple.variable);
        }

        private (Node fromNode, Node toNode) InitializeAnalysis(string fromNodeName, string toNodeName, string variableName)
        {
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {variableName}));
            var fromNode = new Node(fromNodeName);
            var toNode = new Node(toNodeName);
            return (fromNode, toNode);
        }

        public static IEnumerable<object[]> EmptyKillSetActions()
        {
            yield return new object[] {"A", new Edge(null, new ArrayAssignment {ArrayName = "A", Index = "a",RightHandSide = "a+b"}, null)};
            yield return new object[] {"A", new Edge(null, new ArrayDeclaration {ArrayName = "A", ArraySize = 1}, null)};
            yield return new object[] {"x", new Edge(null, new IntDeclaration {VariableName = "x"}, null)};
            yield return new object[] {"x", new Edge(null, new Write{VariableName = "x"}, null) };
            yield return new object[] {"R", new Edge(null, new RecordDeclaration{VariableName = "R"}, null) };
        }

        private ProgramGraph GenerateStandardProgramGraph(HashSet<string> variableNames)
        {
            return new ProgramGraph(variableNames);
        }
    }
}