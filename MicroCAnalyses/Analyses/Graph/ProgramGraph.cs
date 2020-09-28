using System.Collections;
using System.Collections.Generic;
using Analyses;
using Analyses.Analysis.Actions;

namespace Analyses.Graph
{
    public class ProgramGraph
    {
        public HashSet<Node> Nodes { get; private set; }
        public HashSet<Edge> Edges { get; private set; }
        public HashSet<string> VariableNames { get; }
        public const string StartNode = "q_start";
        public const string EndNode = "q_end";

        public ProgramGraph(MicroCTypes.expr ast)
        {
            AstToProgramGraph(ast);
            VariableNames = GetVariables();
        }

        /// <summary>
        /// Constructor for unit tests
        /// </summary>
        /// <param name="variableNames"></param>
        internal ProgramGraph(HashSet<string> variableNames)
        {
            VariableNames = variableNames;
        }
        private void AstToProgramGraph(MicroCTypes.expr ast)
        {
            throw new System.NotImplementedException();
        }

        private HashSet<string> GetVariables()
        {
            HashSet<string> listOfVariables = new HashSet<string>();
            foreach (var edge in Edges)
            {
                switch (edge.Action)
                {
                    case IntDeclaration intDeclaration:
                        listOfVariables.Add(intDeclaration.VariableName);
                        break;
                    
                    case ArrayDeclaration arrayDeclaration:
                        listOfVariables.Add(arrayDeclaration.ArrayName);
                        break;
                    
                    case RecordDeclaration recordDeclaration:
                        listOfVariables.Add($"{recordDeclaration.VariableName}.{RecordMember.Fst}");
                        listOfVariables.Add($"{recordDeclaration.VariableName}.{RecordMember.Snd}");
                        break;
                    
                    default:
                        continue;
                }            
            }

            return listOfVariables;
        }
    }
}