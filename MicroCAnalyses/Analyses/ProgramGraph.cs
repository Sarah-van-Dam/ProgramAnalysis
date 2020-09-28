using System.Collections.Generic;

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
            AstToProgramGraph(ds.Item1, q1, q);
            Nodes.Add(q);
            AstToProgramGraph(ds.Item2, q, q2);
        }

        private void AstToProgramGraph(MicroCTypes.declaration d, Node q1, Node q2)
        {
            Edge e;
            switch (d)
            {
                case MicroCTypes.declaration.ArrD arr:
                    e = new Edge(q1, Action.ArrayDeclaration, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.IntegerD i:
                    e = new Edge(q1, Action.IntDeclaration, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.RecordD r:
                    e = new Edge(q1, Action.RecordDeclaration, q2);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.ContinuedD c:
                    Node q = new Node("Q" + Nodes.Count);
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
                        case MicroCTypes.assignableType.ArrA _:
                            e = new Edge(q1, Action.ArrayAssignment, q2);
                            Edges.Add(e);
                            break;
                        case MicroCTypes.assignableType.RecordEntryA _:
                            e = new Edge(q1, Action.RecordMemberAssignment, q2);
                            Edges.Add(e);
                            break;
                        case MicroCTypes.assignableType.VariableA _:
                            e = new Edge(q1, Action.IntAssignment, q2);
                            Edges.Add(e);
                            break;
                    }
                    break;
                case MicroCTypes.statement.AssignRecord assr:
                    //e = new Edge(q1, Action.recor)
                    break;
                case MicroCTypes.statement.If _if:

                    break;
                case MicroCTypes.statement.IfE _ife:

                    break;
                case MicroCTypes.statement.Read r:

                    break;
                case MicroCTypes.statement.While wh:

                    break;
                case MicroCTypes.statement.Write wr:

                    break;
                case MicroCTypes.statement.ContinuedS c:

                    break;
            }
        }
    }
}