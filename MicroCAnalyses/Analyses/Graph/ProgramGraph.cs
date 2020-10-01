using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public const string NodePrefix = "q";

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

        public override string ToString()
        {
            return $"[\n\tNodes: (\n\t\t{string.Join(",\n\t\t", Nodes.Select(node => node.Name))}\n\t)," +
                    $"\n\tEdges: (\n\t\t{string.Join(",\n\t\t", Edges.Select(edge => edge.ToString()))}\n\t)\n]";
        }

        /// <summary>
        /// Generates a string in .gv file format, to use with Graphviz for showing the program graphs.
        /// </summary>
        /// <returns></returns>
        public string ExportToGV(string graphConfig = "size=\"10\"\n\tnode [shape = circle];")
        {
            return $"digraph program_graph {{\n\t{graphConfig}\n\t{string.Join("\n\t", Edges.Select(edge => $"{edge.FromNode.Name} -> {edge.ToNode.Name} [ label = \"{edge.Action.ToSyntax()}\" ];"))}\n}}";
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

        private void AstToProgramGraph(MicroCTypes.expr ast)
        {
            Nodes = new HashSet<Node>();
            Edges = new HashSet<Edge>();
            var start = new Node(StartNode);
            var end = new Node(EndNode);
            Nodes.Add(start);
            switch (ast)
            {
                case MicroCTypes.expr.DS ds:
                    AstToProgramGraph(ds, start, end);
                    break;
                case MicroCTypes.expr.S s:
                    AstToProgramGraph(s, start, end);
                    break;
            }
            Nodes.Add(end);
        }

        private void AstToProgramGraph(MicroCTypes.expr.DS declStmt, Node qStart, Node qEnd)
        {
            Node qFresh = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh);
            AstToProgramGraph(declStmt.Item1, qStart, qFresh);
            AstToProgramGraph(declStmt.Item2, qFresh, qEnd);
        }

        private void AstToProgramGraph(MicroCTypes.declaration declarations, Node qBeforeDecl, Node qAfterDecl)
        {
            Edge e;
            switch (declarations)
            {
                case MicroCTypes.declaration.ArrD arrayDecl:
                    e = new Edge(qBeforeDecl, new ArrayDeclaration() { ArrayName = arrayDecl.Item1, ArraySize = arrayDecl.Item2 }, qAfterDecl);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.IntegerD integerDecl:
                    e = new Edge(qBeforeDecl, new IntDeclaration() { VariableName = integerDecl.Item }, qAfterDecl);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.RecordD recordDecl:
                    e = new Edge(qBeforeDecl, new RecordDeclaration() { VariableName = recordDecl.Item }, qAfterDecl);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.ContinuedD c:
                    Node qFresh = new Node(NodePrefix + Nodes.Count);
                    Nodes.Add(qFresh);
                    AstToProgramGraph(c.Item1, qBeforeDecl, qFresh);
                    AstToProgramGraph(c.Item2, qFresh, qAfterDecl);
                    break;
            }
        }

        private void AstToProgramGraph(MicroCTypes.expr.S statement, Node qBeforeStmt, Node qAfterStmt)
        {
            AstToProgramGraph(statement.Item, qBeforeStmt, qAfterStmt);
        }

        private void AstToProgramGraph(MicroCTypes.statement statement, Node qBeforeStmt, Node qAfterStmt)
        {
            Edge e;
            switch (statement)
            {
                case MicroCTypes.statement.Assign assignment:
                    switch (assignment.Item1)
                    {
                        case MicroCTypes.assignableType.ArrA arrayAssignment:
                            e = new Edge(qBeforeStmt, new ArrayAssignment()
                            {
                                ArrayName = arrayAssignment.Item1,
                                Index = AstToString(arrayAssignment.Item2),
                                RightHandSide = AstToString(assignment.Item2)
                            }, qAfterStmt);
                            Edges.Add(e);
                            break;
                        case MicroCTypes.assignableType.RecordEntryA recordEntryAssignment:
                            e = new Edge(qBeforeStmt, new RecordMemberAssignment()
                            {
                                RecordName = recordEntryAssignment.Item1,
                                RecordMember = recordEntryAssignment.Item2 == 1 ? RecordMember.Fst : RecordMember.Snd,
                                RightHandSide = AstToString(assignment.Item2)
                            }, qAfterStmt);
                            Edges.Add(e);
                            break;
                        case MicroCTypes.assignableType.VariableA variableAssignment:
                            e = new Edge(qBeforeStmt, new IntAssignment()
                            {
                                VariableName = variableAssignment.Item,
                                RightHandSide = AstToString(assignment.Item2)
                            }, qAfterStmt);
                            Edges.Add(e);
                            break;
                    }
                    break;
                case MicroCTypes.statement.AssignRecord recordAssignment:
                    e = new Edge(qBeforeStmt, new RecordAssignment()
                    {
                        RecordName = recordAssignment.Item1,
                        FirstExpression = AstToString(recordAssignment.Item2),
                        SecondExpression = AstToString(recordAssignment.Item3)
                    }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.If @if:
                    AstToProgramGraph(@if, qBeforeStmt, qAfterStmt);
                    break;
                case MicroCTypes.statement.IfE ifElse:
                    AstToProgramGraph(ifElse, qBeforeStmt, qAfterStmt);
                    break;
                case MicroCTypes.statement.Read read:
                    e = new Edge(qBeforeStmt, new ReadVariable() { VariableName = read.Item }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.While @while:
                    AstToProgramGraph(@while, qBeforeStmt, qAfterStmt);
                    break;
                case MicroCTypes.statement.Write write:
                    e = new Edge(qBeforeStmt, new Write() { VariableName = write.Item }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.ContinuedS c:
                    Node qFresh = new Node(NodePrefix + Nodes.Count);
                    Nodes.Add(qFresh);
                    AstToProgramGraph(c.Item1, qBeforeStmt, qFresh);
                    AstToProgramGraph(c.Item2, qFresh, qAfterStmt);
                    break;
            }
        }

        private void AstToProgramGraph(MicroCTypes.statement.If @if, Node qBeforeIf, Node qAfterIf)
        {
            Node qFresh = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh);
            Edge edge1 = new Edge(qBeforeIf, new Condition() { Cond = AstToString(@if.Item1) }, qFresh);
            Edges.Add(edge1);
            AstToProgramGraph(@if.Item2, qFresh, qAfterIf);
            Edge edge2 = new Edge(qBeforeIf, new Condition() { Cond = $"!({AstToString(@if.Item1)})" }, qAfterIf);
            Edges.Add(edge2);
        }

        private void AstToProgramGraph(MicroCTypes.statement.IfE ifElse, Node qBeforeIfE, Node qAfterIfE)
        {
            Node qFresh1 = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh1);
            Edge edge1 = new Edge(qBeforeIfE, new Condition() { Cond = AstToString(ifElse.Item1) }, qFresh1);
            Edges.Add(edge1);
            AstToProgramGraph(ifElse.Item2, qFresh1, qAfterIfE);
            Node qFresh2 = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh2);
            Edge edge2 = new Edge(qBeforeIfE, new Condition() { Cond = $"!({AstToString(ifElse.Item1)})" }, qFresh2);
            Edges.Add(edge2);
            AstToProgramGraph(ifElse.Item3, qFresh2, qAfterIfE);
        }

        private void AstToProgramGraph(MicroCTypes.statement.While @while, Node qBeforeWhile, Node qAfterWhile)
        {
            Node qFresh = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh);
            Edge edge1 = new Edge(qBeforeWhile, new Condition() { Cond = AstToString(@while.Item1) }, qFresh);
            Edges.Add(edge1);
            AstToProgramGraph(@while.Item2, qFresh, qBeforeWhile);
            Edge edge2 = new Edge(qBeforeWhile, new Condition() { Cond = $"!({AstToString(@while.Item1)})" }, qAfterWhile);
            Edges.Add(edge2);
        }

        /// <summary>
        /// Converts an expression from the AST into a string. Parantheses added for clarity of the ordering.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private string AstToString(MicroCTypes.aExpr expr)
        {
            switch (expr)
            {
                case MicroCTypes.aExpr.Divide opr:
                    return $"({AstToString(opr.Item1)} / {AstToString(opr.Item2)})";
                case MicroCTypes.aExpr.Minus opr:
                    return $"({AstToString(opr.Item1)} - {AstToString(opr.Item2)})";
                case MicroCTypes.aExpr.Modulo opr:
                    return $"({AstToString(opr.Item1)} % {AstToString(opr.Item2)})";
                case MicroCTypes.aExpr.Multiply opr:
                    return $"({AstToString(opr.Item1)} * {AstToString(opr.Item2)})";
                case MicroCTypes.aExpr.Plus opr:
                    return $"({AstToString(opr.Item1)} + {AstToString(opr.Item2)})";
                case MicroCTypes.aExpr.Pow opr:
                    return $"({AstToString(opr.Item1)} ^ {AstToString(opr.Item2)})";
                case MicroCTypes.aExpr.RecordEntry opr:
                    return $"{opr.Item1}.{(opr.Item2 == 1 ? RecordMember.Fst : RecordMember.Snd)}";
                case MicroCTypes.aExpr.Var opr:
                    return $"{opr.Item}";
                case MicroCTypes.aExpr.N n:
                    return n.Item.ToString();
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Converts a boolean expression from the AST into a string. Parantheses added for clarity of the ordering.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private string AstToString(MicroCTypes.bExpr expr)
        {
            switch (expr)
            {
                case MicroCTypes.bExpr.And opr:
                    return $"{AstToString(opr.Item1)} & {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Eq opr:
                    return $"{AstToString(opr.Item1)} == {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Ge opr:
                    return $"{AstToString(opr.Item1)} >= {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Great opr:
                    return $"{AstToString(opr.Item1)} > {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Le opr:
                    return $"{AstToString(opr.Item1)} <= {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Less opr:
                    return $"{AstToString(opr.Item1)} < {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Neq opr:
                    return $"{AstToString(opr.Item1)} != {AstToString(opr.Item2)}";
                case MicroCTypes.bExpr.Not opr:
                    return $"!{AstToString(opr.Item)}";
                case MicroCTypes.bExpr.Or opr:
                    return $"{AstToString(opr.Item1)} | {AstToString(opr.Item2)}";
                default:
                    if (expr == MicroCTypes.bExpr.False) return "false";
                    else if (expr == MicroCTypes.bExpr.True) return "true";
                    else return string.Empty;
            }
        }
    }
}