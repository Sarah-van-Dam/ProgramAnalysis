using Analyses.Algorithms;
using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analyses.Analysis.Distributive
{
    public class DangerousVariables : DistributiveFramework
    {
        private readonly FreeVariablesAnalysis _freeVariablesAnalysis;

        public DangerousVariables(ProgramGraph programGraph, WorklistImplementation worklistImplementation = WorklistImplementation.SortedIteration) : base(programGraph, Direction.Forward, worklistImplementation)
        {
            JoinOperator = Operator.Union;
            _freeVariablesAnalysis = new FreeVariablesAnalysis();
        }

        //start from start node
        public override void InitializeConstraints()
        {
            var startNode = _program.Nodes.Single(n => n.Name == ProgramGraph.StartNode);

            // Handles all nodes except the start node
            foreach (var node in _program.Nodes.Except(new[] { startNode }))
            {
                FinalConstraintsForNodes[node] = new DangerousVariablesConstraint
                {
                    DangerousVariables = new HashSet<string>()
                };
            }

            // Handle the start node
            var temp = new HashSet<string>();
            foreach (var variableName in _program.VariableNames)
            {
                temp.Add(variableName);
            }

            FinalConstraintsForNodes[startNode] = new DangerousVariablesConstraint
            {
                DangerousVariables = new HashSet<string>(temp)
            };

        }

        protected override void AnalysisFunction(Edge edge, IConstraints constraints)
        {
            if (!(constraints is DangerousVariablesConstraint dangerousVariableConstraint))
            {
                throw new Exception($"Something went wrong. It should only be possible to call with {nameof(DangerousVariablesConstraint)}");
            }

            switch (edge.Action)
            {
                case ArrayAssignment arrayAssignment:

                    var freeVariableArrayIndex = _freeVariablesAnalysis.FreeVariables(arrayAssignment.Index);
                    var freeVariableArrayAssignment = _freeVariablesAnalysis.FreeVariables(arrayAssignment.RightHandSide);

                    // Only change the sets, when the index and the assignment variables exists in free variables
                    if (dangerousVariableConstraint.DangerousVariables.Contains(freeVariableArrayIndex.First()) &&
                        dangerousVariableConstraint.DangerousVariables.Contains(freeVariableArrayAssignment.First()))
                    {
                        dangerousVariableConstraint.DangerousVariables.UnionWith(new HashSet<string>{ arrayAssignment.ArrayName });
                    }
                    break;
                case ArrayDeclaration arrayDeclaration:
                    //don't change the set
                    break;
                case Condition condition:
                    //don't change the set
                    break;
                case IntAssignment intAssignment:
                    var freeVariableIntAssignment = _freeVariablesAnalysis.FreeVariables(intAssignment.RightHandSide);
                    // If the assignment free variable exists - add the dangerous variable
                    if (dangerousVariableConstraint.DangerousVariables.Contains(freeVariableIntAssignment.First()))
                    {
                        dangerousVariableConstraint.DangerousVariables.UnionWith(new HashSet<string> { intAssignment.VariableName });
                    }
                    else
                    {
                        // is the assignment free variable does not exist - remove the dangerous variable
                        dangerousVariableConstraint.DangerousVariables.Remove(intAssignment.VariableName);
                    }
                    break;
                case IntDeclaration intDeclaration:
                    //don't change the set
                    break;
                case ReadArray readArray:
                    var freeVariableReadArrayIndex = _freeVariablesAnalysis.FreeVariables(readArray.Index);
                    // If the index free variable exists - add the dangerous variable
                    if (dangerousVariableConstraint.DangerousVariables.Contains(freeVariableReadArrayIndex.First()))
                    {
                        dangerousVariableConstraint.DangerousVariables.UnionWith(new HashSet<string> { readArray.ArrayName });
                    }
                    else
                    {
                        // is the assignment free variable does not exist - remove the dangerous variable
                        dangerousVariableConstraint.DangerousVariables.Remove(readArray.ArrayName);
                    }
                    break;
                case ReadRecordMember readRecordMember:
                    var recordName = $"{readRecordMember.RecordName}.{readRecordMember.RecordMember}";
                    dangerousVariableConstraint.DangerousVariables.Remove(recordName);
                    break;
                case ReadVariable readVariable:
                    dangerousVariableConstraint.DangerousVariables.Remove(readVariable.VariableName);
                    break;
                case RecordAssignment recordAssignment:
                    var recordFst = $"{recordAssignment.RecordName}.{RecordMember.Fst}";
                    var recordSnd = $"{recordAssignment.RecordName}.{RecordMember.Snd}";

                    var freeVariableRecordMemberFst = _freeVariablesAnalysis.FreeVariables(recordAssignment.FirstExpression);
                    var freeVariableRecordMemberSnd = _freeVariablesAnalysis.FreeVariables(recordAssignment.SecondExpression);

                    //if a1 and a2 are in R -> add assignment
                    if (dangerousVariableConstraint.DangerousVariables.Contains(freeVariableRecordMemberFst.First()) &&
                        dangerousVariableConstraint.DangerousVariables.Contains(freeVariableRecordMemberSnd.First()))
                    {
                        dangerousVariableConstraint.DangerousVariables.UnionWith(new HashSet<string> { recordFst, recordSnd });
                    }
                    //only a1 is in R -> remove R.snd add R.fst
                    else if (dangerousVariableConstraint.DangerousVariables.Contains(freeVariableRecordMemberFst.First()) &&
                        !(dangerousVariableConstraint.DangerousVariables.Contains(freeVariableRecordMemberSnd.First())))
                    {
                        dangerousVariableConstraint.DangerousVariables.Add(recordFst);
                        dangerousVariableConstraint.DangerousVariables.Remove(recordSnd);
                    }
                    //only a2 is in R -> remove R.fst add R.snd
                    else if (!(dangerousVariableConstraint.DangerousVariables.Contains(freeVariableRecordMemberFst.First())) &&
                        dangerousVariableConstraint.DangerousVariables.Contains(freeVariableRecordMemberSnd.First()))
                    {
                        dangerousVariableConstraint.DangerousVariables.Remove(recordFst);
                        dangerousVariableConstraint.DangerousVariables.Add(recordSnd);
                    }

                    //a1 and a2 are not in R -> remove assignment
                    else
                    {
                        dangerousVariableConstraint.DangerousVariables.Remove(recordFst);
                        dangerousVariableConstraint.DangerousVariables.Remove(recordSnd);
                    }
                    break;
                case RecordDeclaration recordDeclaration:
                    //don't change the set
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    var recordMemberAssignmentRightHandside = _freeVariablesAnalysis.FreeVariables(recordMemberAssignment.RightHandSide);
                    // If the index free variable exists - add the dangerous variable
                    if (dangerousVariableConstraint.DangerousVariables.Contains(recordMemberAssignmentRightHandside.First()))
                    {
                        dangerousVariableConstraint.DangerousVariables.UnionWith(new HashSet<string> { recordMemberAssignment.RecordMember.ToString() });
                    }
                    else
                    {
                        // is the assignment free variable does not exist - remove the dangerous variable
                        dangerousVariableConstraint.DangerousVariables.Remove(recordMemberAssignment.RecordMember.ToString());
                    }
                    break;
                case Write write:
                    //don't change the set
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }
}
