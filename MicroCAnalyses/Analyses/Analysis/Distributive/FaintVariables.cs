using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Algorithms;
using Analyses.Analysis.Actions;
using Analyses.Graph;

namespace Analyses.Analysis.Distributive
{
    public class FaintVariables : DistributiveFramework
    {
        private readonly FreeVariablesAnalysis _freeVariablesAnalysis;
        
        public FaintVariables(ProgramGraph programGraph, WorklistImplementation worklistImplementation = WorklistImplementation.SortedIteration) : base(programGraph, Direction.Backwards, worklistImplementation)
        {
            Direction = Direction.Backwards;
            JoinOperator = Operator.Union;
            _freeVariablesAnalysis = new FreeVariablesAnalysis();
        }
        
        public override void InitializeConstraints()
        {
            var endNode = _program.Nodes.Single(n => n.Name == ProgramGraph.EndNode);

            // Handles all nodes except the end node
            foreach (var node in _program.Nodes.Except(new []{endNode}))
            {
                FinalConstraintsForNodes[node] = new FaintVariableConstraint
                {
                    StronglyLivedVariables = new HashSet<string>()
                };
            }
            
            // Handle the end node
            FinalConstraintsForNodes[endNode] = new FaintVariableConstraint{
                StronglyLivedVariables = new HashSet<string>()
            };
        }
        
      
        protected override void AnalysisFunction(Edge edge, IConstraints constraints)
        {
            if (!(constraints is FaintVariableConstraint faintVariableConstraint))
            {
                throw new Exception($"Something went wrong. It should only be possible to call with {nameof(FaintVariableConstraint)}");
            }

            switch (edge.Action)
            {
                case ArrayAssignment arrayAssignment:
                    // Only change the sets, when the variables exists in faint/strongly lived variables
                    if (faintVariableConstraint.StronglyLivedVariables.Contains(arrayAssignment.ArrayName))
                    {
                        var freeVariablesOfIndex = _freeVariablesAnalysis.FreeVariables(arrayAssignment.Index);
                        var freeVariablesOfRighthandside =
                            _freeVariablesAnalysis.FreeVariables(arrayAssignment.RightHandSide);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(freeVariablesOfIndex);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(freeVariablesOfRighthandside);
                    }
                    break;
                case ArrayDeclaration arrayDeclaration:
                    faintVariableConstraint.StronglyLivedVariables.Remove(arrayDeclaration.ArrayName);
                    break;
                case Condition condition:
                    var freeVariablesCondition = _freeVariablesAnalysis.FreeVariables(condition.Cond);
                    faintVariableConstraint.StronglyLivedVariables.UnionWith(freeVariablesCondition);
                    break;
                case IntAssignment intAssignment:
                    // Only change the sets, when the variables exists in faint/strongly lived variables
                    if (faintVariableConstraint.StronglyLivedVariables.Contains(intAssignment.VariableName))
                    {
                        var freeVariableIntAssignment =
                            _freeVariablesAnalysis.FreeVariables(intAssignment.RightHandSide);
                        faintVariableConstraint.StronglyLivedVariables.Remove(intAssignment.VariableName);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(freeVariableIntAssignment);
                    }
                    break;
                case IntDeclaration intDeclaration:
                    faintVariableConstraint.StronglyLivedVariables.Remove(intDeclaration.VariableName);
                    break;
                case ReadArray readArray:
                    // Only change the sets, when the variables exists in faint/strongly lived variables
                    if (faintVariableConstraint.StronglyLivedVariables.Contains(readArray.ArrayName))
                    {
                        var freeVariableReadArray = _freeVariablesAnalysis.FreeVariables(readArray.Index);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(freeVariableReadArray);
                    }
                    break;
                case ReadRecordMember readRecordMember:
                    var recordName = $"{readRecordMember.RecordName}.{readRecordMember.RecordMember}";
                    faintVariableConstraint.StronglyLivedVariables.Remove(recordName);
                    break;
                case ReadVariable readVariable:
                    faintVariableConstraint.StronglyLivedVariables.Remove(readVariable.VariableName);
                    break;
                case RecordAssignment recordAssignment:
                    var recordFst = $"{recordAssignment.RecordName}.{RecordMember.Fst}";
                    var recordSnd = $"{recordAssignment.RecordName}.{RecordMember.Snd}";
                    if (faintVariableConstraint.StronglyLivedVariables.Contains(recordFst))
                    {
                        var firstExpressionFreeVariables =
                            _freeVariablesAnalysis.FreeVariables(recordAssignment.FirstExpression);
                        faintVariableConstraint.StronglyLivedVariables.Remove(recordFst);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(firstExpressionFreeVariables);
                    }

                    if (faintVariableConstraint.StronglyLivedVariables.Contains(recordSnd))
                    {
                        var secondExpressionFreeVariables =
                            _freeVariablesAnalysis.FreeVariables(recordAssignment.SecondExpression);
                        faintVariableConstraint.StronglyLivedVariables.Remove(recordSnd);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(secondExpressionFreeVariables);
                        
                    }
                    break;
                case RecordDeclaration recordDeclaration:
                    recordFst = $"{recordDeclaration.VariableName}.{RecordMember.Fst}";
                    recordSnd = $"{recordDeclaration.VariableName}.{RecordMember.Snd}";
                    faintVariableConstraint.StronglyLivedVariables.Add(recordFst);
                    faintVariableConstraint.StronglyLivedVariables.Add(recordSnd);
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    // Only change the sets, when the variables exists in faint/strongly lived variables
                    recordName = $"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}";
                    if (faintVariableConstraint.StronglyLivedVariables.Contains(recordName))
                    {
                        var rhsFreeVariables =
                            _freeVariablesAnalysis.FreeVariables(recordMemberAssignment.RightHandSide);
                        faintVariableConstraint.StronglyLivedVariables.Remove(recordName);
                        faintVariableConstraint.StronglyLivedVariables.UnionWith(rhsFreeVariables);
                    }
                    break;
                case Write write:
                    var freeVariablesOfExpression = _freeVariablesAnalysis.FreeVariables(write.Expression);
                    faintVariableConstraint.StronglyLivedVariables.UnionWith(freeVariablesOfExpression);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
    }
}