using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis.Actions;
using Analyses.Graph;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitions : BitVectorFramework
    {
        public ReachingDefinitions(ProgramGraph programGraph) : base(programGraph)
        {
            Direction = Direction.Forward;
            JoinOperator = Operator.Union;
            InitialiseConstraints();
        }

        private void InitialiseConstraints()
        {
            foreach (var node in _program.Nodes)
            {
                FinalConstraintsForNodes[node] = new ReachingDefinitionConstraints();
            }

            var startingConstraints =
                FinalConstraintsForNodes[_program.Nodes.Single(n => n.Name == ProgramGraph.StartNode)] as ReachingDefinitionConstraints;
            foreach (var variableName in _program.VariableNames)
            {
                startingConstraints.VariableToPossibleAssignments[variableName] =
                    new HashSet<(string variable, string startNode, string endNode)>
                    {
                        (variableName, "?", ProgramGraph.StartNode)
                    };
            }
        }

        public override void Kill(Edge edge, IConstraints constraints)
        {
            if (!(constraints is ReachingDefinitionConstraints rdConstraints))
            {
                throw new Exception("Something went wrong. It should only be possible to call with ReachingDefinitionConstraints");
            }
            switch (edge.Action)
            {
                case IntDeclaration intDeclaration:
                    // A declaration cannot kill anything
                    break;
                case ArrayDeclaration arrayDeclaration:
                    rdConstraints.VariableToPossibleAssignments[arrayDeclaration.ArrayName] =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    break;
                case RecordDeclaration recordDeclaration:
                    // A declaration cannot kill anything
                    break;
                case IntAssignment intAssignment:
                    rdConstraints.VariableToPossibleAssignments[intAssignment.VariableName] =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    break;
                case ArrayAssignment arrayAssignment:
                    // An array assignment cannot kill anything because of amalgamation
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    rdConstraints.VariableToPossibleAssignments[
                            $"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}"] =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    break;
                case RecordAssignment recordAssignment:
                    rdConstraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Fst}"] =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    rdConstraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Snd}"] =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    break;
                case ReadVariable read:
                    rdConstraints.VariableToPossibleAssignments[read.VariableName] =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    break;
                case ReadArray readArray:
                    // A read from an array cannot kill anything because of amalgamation
                    break;
                case ReadRecordMember recordMember:
                    rdConstraints.VariableToPossibleAssignments[$"{recordMember.RecordName}.{recordMember.RecordMember}"]
                        =
                        new HashSet<(string variable, string startNode, string endNode)>();
                    break;
                case Write write:
                    // A write cannot kill anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Action), edge.Action,
                        $"No kill set has been generated for this action: {edge.Action} ");
            }
        }

        public override void Generate(Edge edge, IConstraints constraints)
        {
            if (!(constraints is ReachingDefinitionConstraints rdConstraints))
            {
                throw new Exception("Something went wrong. It should only be possible to call with ReachingDefinitionConstraints");
            }
            switch (edge.Action)
            {
                case IntDeclaration intDeclaration:
                    // A declaration cannot generate anything
                    break;
                case ArrayDeclaration arrayDeclaration:
                    rdConstraints.VariableToPossibleAssignments[arrayDeclaration.ArrayName].Add((
                        arrayDeclaration.ArrayName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case RecordDeclaration recordDeclaration:
                    // A declaration cannot generate anything
                    break;
                case IntAssignment intAssignment:
                    rdConstraints.VariableToPossibleAssignments[intAssignment.VariableName]
                        .Add((intAssignment.VariableName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ArrayAssignment arrayAssignment:
                    rdConstraints.VariableToPossibleAssignments[arrayAssignment.ArrayName]
                        .Add((arrayAssignment.ArrayName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    rdConstraints
                        .VariableToPossibleAssignments[
                            $"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}"]
                        .Add(($"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}",
                            edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case RecordAssignment recordAssignment:
                    rdConstraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Fst}"]
                        .Add(($"{recordAssignment.RecordName}.{RecordMember.Fst}", edge.FromNode.Name,
                            edge.ToNode.Name));
                    rdConstraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Snd}"]
                        .Add(($"{recordAssignment.RecordName}.{RecordMember.Snd}", edge.FromNode.Name,
                            edge.ToNode.Name));
                    break;
                case ReadVariable readVariable:
                    rdConstraints.VariableToPossibleAssignments[readVariable.VariableName]
                        .Add((readVariable.VariableName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ReadArray readArray:
                    rdConstraints.VariableToPossibleAssignments[readArray.ArrayName]
                        .Add((readArray.ArrayName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ReadRecordMember recordMember:
                    rdConstraints.VariableToPossibleAssignments[$"{recordMember.RecordName}.{recordMember.RecordMember}"]
                        .Add(($"{recordMember.RecordName}.{recordMember.RecordMember}", edge.FromNode.Name,
                            edge.ToNode.Name));
                    break;
                case Write write:
                    // A write cannot generate anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Action), edge.Action,
                        $"No gen set has been generated for this action: {edge.Action} ");
            }
        }
    }
}