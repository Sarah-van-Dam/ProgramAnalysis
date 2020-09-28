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
        private ReachingDefinitions _analyses;
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
            _analyses = new ReachingDefinitions(GenerateStandardProgramGraph(variableNames));

            //If it is record declaration there will be two results, otherwise one.
            var originalSize = _analyses.Constraints.VariableToPossibleAssignments.Count;
            var defaultConstraints = new HashSet<(string,string,string)>();
            if (originalSize == 1)
            {
                defaultConstraints = _analyses.Constraints.VariableToPossibleAssignments.Single().Value;
            } else if (originalSize == 2)
            {
                defaultConstraints.UnionWith(_analyses.Constraints.VariableToPossibleAssignments
                    .Single(s => s.Key == $"{variableName}.{RecordMember.Fst}").Value);
                defaultConstraints.UnionWith(_analyses.Constraints.VariableToPossibleAssignments
                    .Single(s => s.Key == $"{variableName}.{RecordMember.Snd}").Value);
            }
            else
            {
                throw new Exception("Original size was not as expected");
            }
            
            _analyses.Kill(edge);

            Assert.Equal(originalSize,_analyses.Constraints.VariableToPossibleAssignments.Count);
            if (originalSize == 1)
            {
                Assert.Equal(defaultConstraints, _analyses.Constraints.VariableToPossibleAssignments.Single().Value);
            }
            else
            {
                var constraints =
                    _analyses.Constraints.VariableToPossibleAssignments
                        .Single(s => s.Key == $"{variableName}.{RecordMember.Fst}").Value
                        .Union(_analyses.Constraints.VariableToPossibleAssignments
                            .Single(s => s.Key == $"{variableName}.{RecordMember.Snd}").Value)
                        .ToHashSet();
                Assert.Equal(defaultConstraints,constraints);
            }
        }

        [Fact]
        public void TestKillOnVariableAssignmentKillsDefaultConstraint()
        {
            _analyses = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string>{"x"}));    
            
            _analyses.Kill(new Edge(null, new IntAssignment{VariableName = "x",RightHandSide = "2+1"}, null));

            var (key, value) = Assert.Single(_analyses.Constraints.VariableToPossibleAssignments);
            Assert.Empty(value);
        }

        [Fact]
        public void TestKillOnRecordMemberAssignmentKillsDefaultConstraintOfMember()
        {
            _analyses = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string>{"R.Fst", "R.Snd"}));
            var originalSndConstraint =
                _analyses.Constraints.VariableToPossibleAssignments.Single(v => v.Key == "R.Snd").Value;

            _analyses.Kill(new Edge(null, new RecordMemberAssignment{RecordName = "R", RecordMember = RecordMember.Fst,RightHandSide = "2+1"}, null));

            var (key, value) = Assert.Single(_analyses.Constraints.VariableToPossibleAssignments.Where( v => v.Key == "R.Fst"));
            Assert.Empty(value);
            Assert.Equal(originalSndConstraint, _analyses.Constraints.VariableToPossibleAssignments.Single(v => v.Key == "R.Snd").Value);
        }
        
        [Fact]
        public void TestKillOnRecordAssignmentKillsDefaultConstraints()
        {
            _analyses = new ReachingDefinitions(GenerateStandardProgramGraph(new HashSet<string>{"R.Fst", "R.Snd"}));    
            
            _analyses.Kill(new Edge(null, new RecordAssignment{RecordName = "R",FirstExpression = "2+1", SecondExpression = "1+2"}, null));

            Assert.Equal(2, _analyses.Constraints.VariableToPossibleAssignments.Count);
            Assert.Empty(_analyses.Constraints.VariableToPossibleAssignments["R.Fst"]);
            Assert.Empty(_analyses.Constraints.VariableToPossibleAssignments["R.Snd"]);
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