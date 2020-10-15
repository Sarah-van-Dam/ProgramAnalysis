using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private readonly List<Node> _defaultNodes;

        public ReachingDefinitionsTest()
        {
            var nodeStart = new Node(ProgramGraph.StartNode);
            var node1 = new Node("q_1");
            var node2 = new Node("q_2");
            var node3 = new Node("q_3");
            var nodeEnd = new Node(ProgramGraph.EndNode);
            _defaultNodes = new List<Node> {nodeStart, node1, node2, node3, nodeEnd};
        }

        [Theory]
        [MemberData(nameof(EmptyKillSetActions))]
        public void TestKillOnEmptyKillSetDoesNotOverrideDefaultSettings(string variableName, Edge edge)
        {
            var variableNames = new HashSet<string> {variableName};
            if (edge.Action is RecordDeclaration)
            {
                variableNames = new HashSet<string>
                    {$"{variableName}.{RecordMember.Fst}", $"{variableName}.{RecordMember.Snd}"};
            }

            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(variableNames, _defaultNodes.ToHashSet(),
                new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();

            //If it is record declaration there will be two results, otherwise one.
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            var originalSize = constraint.VariableToPossibleAssignments.Count;
            var defaultConstraints = new HashSet<(string, string, string)>();
            if (originalSize == 1)
            {
                defaultConstraints = constraint.VariableToPossibleAssignments.Single().Value;
            }
            else if (originalSize == 2)
            {
                defaultConstraints.UnionWith(constraint.VariableToPossibleAssignments
                    .Single(s => s.Key == $"{variableName}.{RecordMember.Fst}").Value);
                defaultConstraints.UnionWith(constraint.VariableToPossibleAssignments
                    .Single(s => s.Key == $"{variableName}.{RecordMember.Snd}").Value);
            }
            else
            {
                throw new Exception("Original size was not as expected");
            }

            _analysis.Kill(edge, constraint);

            Assert.Equal(originalSize, constraint.VariableToPossibleAssignments.Count);
            if (originalSize == 1)
            {
                Assert.Equal(defaultConstraints, constraint.VariableToPossibleAssignments.Single().Value);
            }
            else
            {
                var constraints =
                    constraint.VariableToPossibleAssignments
                        .Single(s => s.Key == $"{variableName}.{RecordMember.Fst}").Value
                        .Union(constraint.VariableToPossibleAssignments
                            .Single(s => s.Key == $"{variableName}.{RecordMember.Snd}").Value)
                        .ToHashSet();
                Assert.Equal(defaultConstraints, constraints);
            }
        }

        [Fact]
        public void TestKillOnVariableAssignmentKillsDefaultConstraint()
        {
            var plus = MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewNumber(1),
                MicroCTypes.arithmeticExpression.NewNumber(2));
            var edge = new Edge(new Node("q_start"), new IntAssignment {VariableName = "x", RightHandSide = plus},
                new Node("q_end"));
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {"x"},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;

            _analysis.Kill(edge, constraint);

            var (key, value) = Assert.Single(constraint.VariableToPossibleAssignments);
            Assert.Empty(value);
        }


        [Fact]
        public void TestKillOnRecordMemberAssignmentKillsDefaultConstraintOfMember()
        {
            var plus = MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewNumber(2),
                MicroCTypes.arithmeticExpression.NewNumber(1));
            var edge = new Edge(new Node("q_start"),
                new RecordMemberAssignment {RecordName = "R", RecordMember = RecordMember.Fst, RightHandSide = plus},
                new Node("q_end"));
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {"R.Fst", "R.Snd"},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            var originalSndConstraint =
                constraint.VariableToPossibleAssignments.Single(v => v.Key == "R.Snd").Value;

            _analysis.Kill(edge, constraint);

            var (key, value) = Assert.Single(constraint.VariableToPossibleAssignments.Where(v => v.Key == "R.Fst"));
            Assert.Empty(value);
            Assert.Equal(originalSndConstraint,
                constraint.VariableToPossibleAssignments.Single(v => v.Key == "R.Snd").Value);
        }

        [Fact]
        public void TestKillOnRecordAssignmentKillsDefaultConstraints()
        {
            var plusFirstExpr = MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewNumber(2),
                MicroCTypes.arithmeticExpression.NewNumber(1));
            var plusSecondExpr = MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewNumber(1),
                MicroCTypes.arithmeticExpression.NewNumber(2));
            var edge = new Edge(new Node("q_start"),
                new RecordAssignment {RecordName = "R", FirstExpression = plusFirstExpr, SecondExpression = plusSecondExpr},
                new Node("q_end"));
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {"R.Fst", "R.Snd"},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;

            _analysis.Kill(edge, constraint);

            Assert.Equal(2, constraint.VariableToPossibleAssignments.Count);
            Assert.Empty(constraint.VariableToPossibleAssignments["R.Fst"]);
            Assert.Empty(constraint.VariableToPossibleAssignments["R.Snd"]);
        }


        [Fact]
        public void TestGenerateOnAssignmentsAddVariableNameAndNodesToTuple()
        {
            var edge = new Edge(_defaultNodes[0], new IntAssignment
                {
                    VariableName = SingleVariableName, RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(
                        2)
                },
                _defaultNodes[1]);
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {SingleVariableName},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            _analysis.Kill(edge, constraint);

            _analysis.Generate(edge, constraint);

            var tuples = Assert.Single(constraint.VariableToPossibleAssignments).Value;
            var insertedTuple = tuples.Single();
            Assert.Equal(_defaultNodes[0].Name, insertedTuple.startNode);
            Assert.Equal(_defaultNodes[1].Name, insertedTuple.endNode);
            Assert.Equal(SingleVariableName, insertedTuple.variable);
        }

        [Fact]
        public void TestGenerateOnArrayAssignmentsAddVariableNameAndNodesToTuple()
        {
            
            const string arrayName = "A";
            var edge = new Edge(_defaultNodes[1],
                new ArrayAssignment {ArrayName = arrayName, Index = MicroCTypes.arithmeticExpression.NewNumber(1), RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(2)},
                _defaultNodes[2]);
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {arrayName},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            GivenArrayHasBeenDeclared(arrayName, _defaultNodes[0].Name, _defaultNodes[1].Name, constraint);

            _analysis.Generate(edge, constraint);

            var kvp = Assert.Single(constraint.VariableToPossibleAssignments);
            Assert.Equal(2, kvp.Value.Count);
            var constraints = constraint.VariableToPossibleAssignments[arrayName];
            Assert.Contains((arrayName, _defaultNodes[1].Name, _defaultNodes[2].Name), constraints);
            Assert.Contains((arrayName, _defaultNodes[0].Name, _defaultNodes[1].Name), constraints);
        }

        [Fact]
        public void TestGenerateOnRecordAssignmentsAddVariableNameAndNodesToTuple()
        {
            const string recordName = "R";
            var recordFirst = $"{recordName}.{RecordMember.Fst}";
            var recordSecond = $"{recordName}.{RecordMember.Snd}";

            var edge = new Edge(_defaultNodes[0],
                new RecordAssignment {RecordName = recordName, FirstExpression = MicroCTypes.arithmeticExpression.NewNumber(1), SecondExpression = MicroCTypes.arithmeticExpression.NewNumber(2)},
                _defaultNodes[1]);
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(
                new HashSet<string> {recordFirst, recordSecond}, _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            _analysis.Kill(edge, constraint);

            _analysis.Generate(edge, constraint);

            Assert.Equal(2, constraint.VariableToPossibleAssignments.Count);
            var firstConstraint =
                Assert.Single(
                    constraint.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordFirst)).Value;
            var firstTuple = firstConstraint.Single();
            Assert.Equal(_defaultNodes[0].Name, firstTuple.startNode);
            Assert.Equal(_defaultNodes[1].Name, firstTuple.endNode);
            Assert.Equal(recordFirst, firstTuple.variable);

            var secondConstraint =
                Assert.Single(
                    constraint.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordSecond)).Value;
            var secondTuple = secondConstraint.Single();
            Assert.Equal(_defaultNodes[0].Name, secondTuple.startNode);
            Assert.Equal(_defaultNodes[1].Name, secondTuple.endNode);
            Assert.Equal(recordSecond, secondTuple.variable);
        }

        [Fact]
        public void TestGenerateOnRecordMemberAssignmentsAddVariableNameAndNodesToTuple()
        {
            const string recordName = "R";
            var recordFirst = $"{recordName}.{RecordMember.Fst}";
            var recordSecond = $"{recordName}.{RecordMember.Snd}";

            var edge = new Edge(_defaultNodes[0],
                new RecordMemberAssignment
                    {RecordName = recordName, RecordMember = RecordMember.Fst, RightHandSide = MicroCTypes.arithmeticExpression.NewNumber(1)},
                _defaultNodes[1]);
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(
                new HashSet<string> {recordFirst, recordSecond}, _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            _analysis.Kill(edge, constraint);

            _analysis.Generate(edge, constraint);

            Assert.Equal(2, constraint.VariableToPossibleAssignments.Count);
            var firstConstraint =
                Assert.Single(
                    constraint.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordFirst)).Value;
            var firstTuple = firstConstraint.Single();
            Assert.Equal(_defaultNodes[0].Name, firstTuple.startNode);
            Assert.Equal(_defaultNodes[1].Name, firstTuple.endNode);
            Assert.Equal(recordFirst, firstTuple.variable);

            var secondConstraint =
                Assert.Single(
                    constraint.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordSecond)).Value;
            var secondTuple = secondConstraint.Single();
            Assert.Equal("?", secondTuple.startNode);
            Assert.Equal(ProgramGraph.StartNode, secondTuple.endNode);
            Assert.Equal(recordSecond, secondTuple.variable);
        }

        [Fact]
        public void TestGenerateOnReadRecordMemberAddVariableNameAndNodesToTuple()
        {
            const string recordName = "R";
            var recordFirst = $"{recordName}.{RecordMember.Fst}";
            var recordSecond = $"{recordName}.{RecordMember.Snd}";

            var edge = new Edge(_defaultNodes[0],
                new ReadRecordMember {RecordName = recordName, RecordMember = RecordMember.Fst},
                _defaultNodes[1]);
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(
                new HashSet<string> {recordFirst, recordSecond}, _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();

            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            _analysis.Kill(edge, constraint);

            _analysis.Generate(edge, constraint);

            Assert.Equal(2, constraint.VariableToPossibleAssignments.Count);
            var firstConstraint =
                Assert.Single(
                    constraint.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordFirst)).Value;
            var firstTuple = firstConstraint.Single();
            Assert.Equal(_defaultNodes[0].Name, firstTuple.startNode);
            Assert.Equal(_defaultNodes[1].Name, firstTuple.endNode);
            Assert.Equal(recordFirst, firstTuple.variable);

            var secondConstraint =
                Assert.Single(
                    constraint.VariableToPossibleAssignments.Where(r =>
                        r.Key == recordSecond)).Value;
            var secondTuple = secondConstraint.Single();
            Assert.Equal("?", secondTuple.startNode);
            Assert.Equal(ProgramGraph.StartNode, secondTuple.endNode);
            Assert.Equal(recordSecond, secondTuple.variable);
        }

        [Fact]
        public void TestGenerateOnReadVariableAddsVariableNameAndNodesToTuple()
        {
            var edge = new Edge(_defaultNodes[0], new ReadVariable {VariableName = SingleVariableName},
                _defaultNodes[1]);
            _defaultNodes[0].OutGoingEdges.Add(edge);
            _defaultNodes[1].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {SingleVariableName},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            _analysis.Kill(edge, constraint);

            _analysis.Generate(edge, constraint);

            var tuples = Assert.Single(constraint.VariableToPossibleAssignments).Value;
            var insertedTuple = tuples.Single();
            Assert.Equal(_defaultNodes[0].Name, insertedTuple.startNode);
            Assert.Equal(_defaultNodes[1].Name, insertedTuple.endNode);
            Assert.Equal(SingleVariableName, insertedTuple.variable);
        }

        [Fact]
        public void TestGenerateOnReadArrayIndexAddsArrayNameAndNodesToTuple()
        {
            const string arrayName = "A";
            var edge = new Edge(_defaultNodes[1], new ReadArray {ArrayName = arrayName, Index = MicroCTypes.arithmeticExpression.NewVariable("n")},
                _defaultNodes[2]);
            _defaultNodes[1].OutGoingEdges.Add(edge);
            _defaultNodes[2].InGoingEdges.Add(edge);
            _analysis = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string> {arrayName},
                _defaultNodes.ToHashSet(), new HashSet<Edge> {edge}));
            _analysis.InitializeConstraints();
            var constraint = _analysis.Constraints[_defaultNodes[0]] as ReachingDefinitionConstraints;
            GivenArrayHasBeenDeclared(arrayName, _defaultNodes[0].Name, _defaultNodes[1].Name, constraint);
            _analysis.Kill(edge, constraint);

            _analysis.Generate(edge, constraint);

            var kvp = Assert.Single(constraint.VariableToPossibleAssignments);
            Assert.Equal(2, kvp.Value.Count);
            var constraints = constraint.VariableToPossibleAssignments[arrayName];
            Assert.Contains((arrayName, _defaultNodes[1].Name, _defaultNodes[2].Name), constraints);
            Assert.Contains((arrayName, _defaultNodes[0].Name, _defaultNodes[1].Name), constraints);
        }

        public static IEnumerable<object[]> EmptyKillSetActions()
        {
            var plus = MicroCTypes.arithmeticExpression.NewPlus(MicroCTypes.arithmeticExpression.NewVariable("a"),
                MicroCTypes.arithmeticExpression.NewVariable("b"));
            yield return new object[]
            {
                "A",
                new Edge(new Node("q_start"), new ArrayAssignment {ArrayName = "A", Index = MicroCTypes.arithmeticExpression.NewVariable("a"), RightHandSide = plus},
                    new Node("q_end"))
            };
            yield return new object[]
                {"x", new Edge(new Node("q_start"), new IntDeclaration {VariableName = "x"}, new Node("q_end"))};
            yield return new object[]
                {"x", new Edge(new Node("q_start"), new Write {Expression = MicroCTypes.arithmeticExpression.NewVariable("x")}, new Node("q_end"))};
            yield return new object[]
                {"R", new Edge(new Node("q_start"), new RecordDeclaration {VariableName = "R"}, new Node("q_end"))};
        }


        private void GivenArrayHasBeenDeclared(string arrayName, string declarationStartNode, string declarationEndNode,
            ReachingDefinitionConstraints constraints)
        {
            var fromNode = new Node(declarationStartNode);
            var toNode = new Node(declarationEndNode);
            var edge = new Edge(fromNode, new ArrayDeclaration {ArrayName = arrayName, ArraySize = 2},
                toNode);
            _analysis.Kill(edge, constraints);
            _analysis.Generate(edge, constraints);
        }

        private ProgramGraph GenerateStandardProgramGraph(HashSet<string> variableNames, HashSet<Node> nodes,
            HashSet<Edge> edges)
        {
            return new ProgramGraph(variableNames, nodes, edges);
        }
    }
}