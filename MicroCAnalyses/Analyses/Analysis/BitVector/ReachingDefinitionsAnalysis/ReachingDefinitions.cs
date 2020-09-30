using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Analyses.Analysis.Actions;
using Analyses.Graph;

[assembly: InternalsVisibleTo("Analyses.Test")]
namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitions : BitVectorFramework
    {
        private readonly ReachingDefinitionConstraints _constraints;

        internal override Constraints Constraints => _constraints;
        public ReachingDefinitions(ProgramGraph programGraph)
        {
            Direction = Direction.Forward;
            JoinOperator = Operator.Union;
            _program = programGraph;
            _constraints = new ReachingDefinitionConstraints();
            InitialiseConstraints();
        }

        private void InitialiseConstraints()
        {
            foreach (var variableName in _program.VariableNames)
            {
                _constraints.VariableToPossibleAssignments[variableName] = 
                    new HashSet<(string action, string startNode, string endNode)>
                    {
                        (variableName, "?", ProgramGraph.StartNode)
                    };
            }

        }

        public override void Kill(Edge edge)
        {
            switch (edge.Action)
            {
                case IntDeclaration intDeclaration:
                    // A declaration cannot kill anything
                    break;
                case ArrayDeclaration arrayDeclaration:
                    // A declaration cannot kill anything
                    break;
                case RecordDeclaration recordDeclaration:
                    // A declaration cannot kill anything
                    break;
                case IntAssignment intAssignment:
                    _constraints.VariableToPossibleAssignments[intAssignment.VariableName] = 
                        new HashSet<(string action, string startNode, string endNode)>();
                    break;
                case ArrayAssignment arrayAssignment:
                    // An array assignment cannot kill anything because of amalgamation
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    _constraints.VariableToPossibleAssignments[$"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}"] = 
                        new HashSet<(string action, string startNode, string endNode)>();
                    break;
                case RecordAssignment recordAssignment:
                    _constraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Fst}"] = 
                        new HashSet<(string action, string startNode, string endNode)>();
                    _constraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Snd}"] =
                        new HashSet<(string action, string startNode, string endNode)>();
                    break;
                case ReadVariable read:
                    _constraints.VariableToPossibleAssignments[read.VariableName] = new HashSet<(string action, string startNode, string endNode)>();
                    break;
                case ReadArray readArray:
                    // A read from an array cannot kill anything because of amalgamation
                    break;
                case ReadRecordMember recordMember:
                    _constraints.VariableToPossibleAssignments[$"{recordMember.RecordName}.{recordMember.RecordMember}"] = 
                        new HashSet<(string action, string startNode, string endNode)>();
                    break;
                case Write write:
                    // A write cannot kill anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Action), edge.Action,
                        $"No kill set has been generated for this action: {edge.Action} ");
            }
        }

        public override void Generate(Edge edge)
        {
            switch (edge.Action)
            {
                case IntDeclaration intDeclaration:
                    // A declaration cannot generate anything
                    break;
                case ArrayDeclaration arrayDeclaration:
                    // A declaration cannot generate anything
                    break;
                case RecordDeclaration recordDeclaration:
                    // A declaration cannot generate anything
                    break;
                case IntAssignment intAssignment:
                    _constraints.VariableToPossibleAssignments[intAssignment.VariableName]
                        .Add((intAssignment.VariableName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ArrayAssignment arrayAssignment:
                    _constraints.VariableToPossibleAssignments[arrayAssignment.ArrayName]
                        .Add((arrayAssignment.ArrayName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    _constraints.VariableToPossibleAssignments[$"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}"]
                        .Add(($"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}", edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case RecordAssignment recordAssignment:
                    _constraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Fst}"]
                        .Add(($"{recordAssignment.RecordName}.{RecordMember.Fst}", edge.FromNode.Name, edge.ToNode.Name));
                    _constraints.VariableToPossibleAssignments[$"{recordAssignment.RecordName}.{RecordMember.Snd}"]
                        .Add(($"{recordAssignment.RecordName}.{RecordMember.Snd}", edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ReadVariable readVariable:
                    _constraints.VariableToPossibleAssignments[readVariable.VariableName]
                        .Add((readVariable.VariableName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ReadArray readArray:
                    _constraints.VariableToPossibleAssignments[readArray.ArrayName]
                        .Add((readArray.ArrayName, edge.FromNode.Name, edge.ToNode.Name));
                    break;
                case ReadRecordMember recordMember:
                    _constraints.VariableToPossibleAssignments[$"{recordMember.RecordName}.{recordMember.RecordMember}"]
                        .Add(($"{recordMember.RecordName}.{recordMember.RecordMember}", edge.FromNode.Name, edge.ToNode.Name));                    
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