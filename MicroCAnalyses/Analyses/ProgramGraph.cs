using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis;
using Analyses.Analysis.Actions;
using Analyses.Graph;

namespace Analyses
{
    public class ProgramGraph
    {
        public HashSet<Node> Nodes { get; private set; }
        public HashSet<Edge> Edges { get; private set; }

        public ProgramGraph(MicroCTypes.expr ast)
        {
            AstToProgramGraph(ast);
        }


        public ProgramGraph(HashSet<string> variableNames) {
            AstToProgramGraph(Parser.parse(string.Join(";\n", variableNames)));
        }

        private void AstToProgramGraph(MicroCTypes.expr ast)
        {
            var start = new Node("Q_Start");
            var end = new Node("Q_End");
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

        private void AstToProgramGraph(MicroCTypes.expr.DS ds, Node q1, Node q2)
        {
            Node q = new Node("Q" + Nodes.Count);
            Nodes.Add(q);
            AstToProgramGraph(ds.Item1, q1, q);
            AstToProgramGraph(ds.Item2, q, q2);
        }

        private void AstToProgramGraph(MicroCTypes.declaration d, Node q1, Node q2)
        {
            Edge e;
            switch (d)
            {
                case MicroCTypes.declaration.ArrD arr:
                    e = new Edge(q1, new ArrayDeclaration() { ArrayName = arr.Item1, ArraySize = arr.Item2 }, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.IntegerD i:
                    e = new Edge(q1, new IntDeclaration() { VariableName = i.Item }, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.RecordD r:
                    e = new Edge(q1, new RecordDeclaration() { VariableName = r.Item }, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.ContinuedD c:
                    Node q = new Node("Q" + Nodes.Count);
                    Nodes.Add(q);
                    AstToProgramGraph(c.Item1, q1, q);
                    AstToProgramGraph(c.Item2, q, q2);
                    break;
            }
        }

        private void AstToProgramGraph(MicroCTypes.expr.S s, Node q1, Node q2)
        {
            AstToProgramGraph(s.Item, q1, q2);
        }

        private void AstToProgramGraph(MicroCTypes.statement s, Node q1, Node q2)
        {
            Edge e;
            switch (s)
            {
                case MicroCTypes.statement.Assign ass:
                    switch (ass.Item1)
                    {
                        case MicroCTypes.assignableType.ArrA assignArray:
                            e = new Edge(q1, new ArrayAssignment() {
                                ArrayName = assignArray.Item1,
                                Index = AstToString(assignArray.Item2),
                                RightHandSide = AstToString(ass.Item2)
                            }, q2);
                            Edges.Add(e);
                            break;
                        case MicroCTypes.assignableType.RecordEntryA assignRecord:
                            e = new Edge(q1, new RecordMemberAssignment() {
                                RecordName = assignRecord.Item1,
                                RecordMember = assignRecord.Item2 > 1 ? RecordMember.Snd : RecordMember.Fst,
                                RightHandSide = AstToString(ass.Item2)
                            }, q2);
                            Edges.Add(e);
                            break;
                        case MicroCTypes.assignableType.VariableA assignVar:
                            e = new Edge(q1, new IntAssignment() {
                                VariableName = assignVar.Item,
                                RightHandSide = AstToString(ass.Item2)
                            }, q2);
                            Edges.Add(e);
                            break;
                    }
                    break;
                case MicroCTypes.statement.AssignRecord assr:
                    e = new Edge(q1, new RecordAssignment() {
                        RecordName = assr.Item1,
                        FirstExpression = AstToString(assr.Item2),
                        SecondExpression = AstToString(assr.Item3)
                    }, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.If _if:
                    AstToProgramGraph(_if, q1, q2);
                    break;
                case MicroCTypes.statement.IfE _ife:
                    AstToProgramGraph(_ife, q1, q2);
                    break;
                case MicroCTypes.statement.Read r:
                    e = new Edge(q1, new ReadVariable() { VariableName = r.Item }, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.While wh:
                    AstToProgramGraph(wh, q1, q2);
                    break;
                case MicroCTypes.statement.Write wr:
                    e = new Edge(q1, new Write() { VariableName = wr.Item }, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.ContinuedS c:
                    Node q = new Node("Q" + Nodes.Count);
                    Nodes.Add(q);
                    AstToProgramGraph(c.Item1, q1, q);
                    AstToProgramGraph(c.Item2, q, q2);
                    break;
            }
        }

        private void AstToProgramGraph(MicroCTypes.statement.If _if, Node q1, Node q2)
        {
            Node q = new Node("Q" + Nodes.Count);
            Nodes.Add(q);
            Edge e = new Edge(q1, new Condition() { Cond = AstToString(_if.Item1) }, q);
            Edges.Add(e);
            AstToProgramGraph(_if.Item2, q, q2);
            e = new Edge(q1, new Condition() { Cond = $"!({AstToString(_if.Item1)})" }, q2);
            Edges.Add(e);
        }

        private void AstToProgramGraph(MicroCTypes.statement.IfE _ife, Node q1, Node q2)
        {
            Node q = new Node("Q" + Nodes.Count);
            Nodes.Add(q);
            Edge e = new Edge(q1, new Condition() { Cond = AstToString(_ife.Item1) }, q);
            Edges.Add(e);
            AstToProgramGraph(_ife.Item2, q, q2);
            q = new Node("Q" + Nodes.Count);
            Nodes.Add(q);
            e = new Edge(q1, new Condition() { Cond = $"!({AstToString(_ife.Item1)})" }, q);
            Edges.Add(e);
            AstToProgramGraph(_ife.Item3, q, q2);
        }

        private void AstToProgramGraph(MicroCTypes.statement.While wh, Node q1, Node q2)
        {
            Node q = new Node("Q" + Nodes.Count);
            Nodes.Add(q);
            Edge e = new Edge(q1, new Condition() { Cond = AstToString(wh.Item1) }, q);
            Edges.Add(e);
            AstToProgramGraph(wh.Item2, q, q1);
            e = new Edge(q1, new Condition() { Cond = $"!({AstToString(wh.Item1)})" }, q2);
            Edges.Add(e);
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
                    return $"{opr.Item1}({(opr.Item2 > 1 ? "snd" : "fst")})";
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