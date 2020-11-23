using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis.Actions;
using Analyses.Helpers;

namespace Analyses.Graph
{
    public class ProgramGraph
    {
        public HashSet<Node> Nodes { get; private set; } = new HashSet<Node>();
        public HashSet<Edge> Edges { get; private set; } = new HashSet<Edge>();
        public HashSet<string> VariableNames { get; }
        public const string StartNode = "q_start";
        public const string EndNode = "q_end";
        public const string NodePrefix = "q";

        public ProgramGraph(MicroCTypes.expressionTree ast)
        {
            AstToProgramGraph(ast);
            VariableNames = GetVariables();

            LinkNodes();
            SortNodes();
        }

        /// <summary>
        /// Constructor for unit tests
        /// </summary>
        /// <param name="variableNames"></param>
        internal ProgramGraph(HashSet<string> variableNames, HashSet<Node> nodes, HashSet<Edge> edges)
        {
            VariableNames = variableNames;
            Nodes = nodes;
            Edges = edges;
        }

        public override string ToString()
        {
            return $"[\n\tNodes: (\n\t\t{string.Join(",\n\t\t", Nodes.Select(node => node.Name))}\n\t)," +
                   $"\n\tEdges: (\n\t\t{string.Join(",\n\t\t", Edges.Select(edge => edge.ToSyntax()))}\n\t)\n]";
        }

        /// <summary>
        /// Generates a string in .gv file format, to use with Graphviz for showing the program graphs.
        /// </summary>
        /// <returns></returns>
        public string ExportToGV(string graphConfig = "size=\"10\"\n\tnode [shape = circle];")
        {
            return
                $"digraph program_graph {{\n\t{graphConfig}\n\t{string.Join("\n\t", Edges.Select(edge => $"{edge.FromNode.Name} -> {edge.ToNode.Name} [ label = \"{edge.Action.ToSyntax()}\" ];"))}\n}}";
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

        private void LinkNodes()
        {
            foreach (Edge edge in Edges)
            {
                edge.FromNode.OutGoingEdges.Add(edge);
                edge.ToNode.InGoingEdges.Add(edge);
            }
        }

        /// <summary>
        /// Rebuilds the node and edge structure, while changing the node names to be ascending based on their distance to the entry node.
        /// </summary>
        private void SortNodes()
        {
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(Nodes.First());
            HashSet<Node> nodes = new HashSet<Node>();
            HashSet<Edge> edges = new HashSet<Edge>();
            Dictionary<Node, Node> nodeMapping = new Dictionary<Node, Node>();

            while (queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();
                if (nodeMapping.ContainsKey(currentNode)) continue; // Skip already visited nodes.

                IEnumerable<Node> childNodes = currentNode.OutGoingEdges.Select(edge => edge.ToNode);
                foreach (Node childNode in childNodes)
                    queue.Enqueue(childNode);

                if (currentNode.Name == StartNode)
                    nodeMapping[currentNode] = new Node(StartNode);
                else if (currentNode.Name == EndNode)
                    continue; // Skip the end node to force it to after the while-loop.
                else
                    nodeMapping[currentNode] = new Node(NodePrefix + nodeMapping.Keys.Count);
            }

            nodeMapping[Nodes.Last()] = new Node(EndNode);

            foreach (Edge edge in Edges)
            {
                Edge newEdge = new Edge(nodeMapping[edge.FromNode], edge.Action, nodeMapping[edge.ToNode]);
                edges.Add(newEdge);
            }

            foreach (Node newNode in nodeMapping.Values)
                nodes.Add(newNode);

            Nodes = nodes;
            Edges = edges;
            LinkNodes();
        }

        private void AstToProgramGraph(MicroCTypes.expressionTree ast)
        {
            Nodes = new HashSet<Node>();
            Edges = new HashSet<Edge>();
            var start = new Node(StartNode);
            var end = new Node(EndNode);
            Nodes.Add(start);
            switch (ast)
            {
                case MicroCTypes.expressionTree.DS ds:
                    AstToProgramGraph(ds, start, end);
                    break;
                case MicroCTypes.expressionTree.S s:
                    AstToProgramGraph(s, start, end);
                    break;
            }

            Nodes.Add(end);
        }

        private void AstToProgramGraph(MicroCTypes.expressionTree.DS declStmt, Node qStart, Node qEnd)
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
                case MicroCTypes.declaration.ArrayDeclaration arrayDecl:
                    e = new Edge(qBeforeDecl,
                        new ArrayDeclaration() {ArrayName = arrayDecl.Item1, ArraySize = arrayDecl.Item2}, qAfterDecl);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.IntegerDeclaration integerDecl:
                    e = new Edge(qBeforeDecl, new IntDeclaration() {VariableName = integerDecl.Item}, qAfterDecl);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.RecordDeclaration recordDecl:
                    e = new Edge(qBeforeDecl, new RecordDeclaration() {VariableName = recordDecl.Item}, qAfterDecl);
                    Edges.Add(e);
                    break;
                case MicroCTypes.declaration.ContinuedDeclaration c:
                    Node qFresh = new Node(NodePrefix + Nodes.Count);
                    Nodes.Add(qFresh);
                    AstToProgramGraph(c.Item1, qBeforeDecl, qFresh);
                    AstToProgramGraph(c.Item2, qFresh, qAfterDecl);
                    break;
            }
        }

        private void AstToProgramGraph(MicroCTypes.expressionTree.S statement, Node qBeforeStmt, Node qAfterStmt)
        {
            AstToProgramGraph(statement.Item, qBeforeStmt, qAfterStmt);
        }

        private void AstToProgramGraph(MicroCTypes.statement statement, Node qBeforeStmt, Node qAfterStmt)
        {
            Edge e;
            switch (statement)
            {
                case MicroCTypes.statement.AssignVariable assignment:
                    e = new Edge(qBeforeStmt, new IntAssignment
                    {
                        VariableName = assignment.Item1,
                        RightHandSide = assignment.Item2
                    }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.AssignArray arrayAssignment:
                    e = new Edge(qBeforeStmt, new ArrayAssignment()
                    {
                        ArrayName = arrayAssignment.Item1,
                        Index = arrayAssignment.Item2,
                        RightHandSide = arrayAssignment.Item3
                    }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.AssignRecordMember recordEntryAssignment:
                    e = new Edge(qBeforeStmt, new RecordMemberAssignment()
                    {
                        RecordName = recordEntryAssignment.Item1,
                        RecordMember = recordEntryAssignment.Item3 == 1 ? RecordMember.Fst : RecordMember.Snd,
                        RightHandSide = recordEntryAssignment.Item2
                    }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.AssignRecord recordAssignment:
                    e = new Edge(qBeforeStmt, new RecordAssignment()
                    {
                        RecordName = recordAssignment.Item1,
                        FirstExpression = recordAssignment.Item2,
                        SecondExpression = recordAssignment.Item3
                    }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.If @if:
                    AstToProgramGraph(@if, qBeforeStmt, qAfterStmt);
                    break;
                case MicroCTypes.statement.IfElse ifElse:
                    AstToProgramGraph(ifElse, qBeforeStmt, qAfterStmt);
                    break;
                case MicroCTypes.statement.ReadVariable read:
                    e = new Edge(qBeforeStmt, new ReadVariable() {VariableName = read.Item}, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.ReadArray read:
                    e = new Edge(qBeforeStmt, new ReadArray() {ArrayName = read.Item1, Index = read.Item2}, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.ReadRecordMember read:
                    e = new Edge(qBeforeStmt,
                        new ReadRecordMember()
                        {
                            RecordName = read.Item1,
                            RecordMember = read.Item2 == 1 ? RecordMember.Fst : RecordMember.Snd
                        }, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.While @while:
                    AstToProgramGraph(@while, qBeforeStmt, qAfterStmt);
                    break;
                case MicroCTypes.statement.Write write:
                    e = new Edge(qBeforeStmt, new Write() {Expression = write.Item}, qAfterStmt);
                    Edges.Add(e);
                    break;
                case MicroCTypes.statement.ContinuedStatement c:
                    Node qFresh = new Node(NodePrefix + Nodes.Count);
                    Nodes.Add(qFresh);
                    AstToProgramGraph(c.Item1, qBeforeStmt, qFresh);
                    AstToProgramGraph(c.Item2, qFresh, qAfterStmt);
                    break;
            }
        }

        private void AstToProgramGraph(MicroCTypes.statement.If @if, Node qBeforeIf, Node qAfterIf)
        {
            var qFresh = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh);
            var edge1 = new Edge(qBeforeIf,
                new Condition() {Cond = @if.Item1, GraphvizSyntax = AstExtensions.AstToString(@if.Item1)}, qFresh);
            Edges.Add(edge1);
            AstToProgramGraph(@if.Item2, qFresh, qAfterIf);
            var edge2 = new Edge(qBeforeIf,
                new Condition() {Cond = MicroCTypes.booleanExpression.NewNot(@if.Item1), GraphvizSyntax = $"!({AstExtensions.AstToString(@if.Item1)})"}, qAfterIf);
            Edges.Add(edge2);
        }

        private void AstToProgramGraph(MicroCTypes.statement.IfElse ifElse, Node qBeforeIfE, Node qAfterIfE)
        {
            Node qFresh1 = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh1);
            Edge edge1 = new Edge(qBeforeIfE,
                new Condition() {Cond = ifElse.Item1, GraphvizSyntax = AstExtensions.AstToString(ifElse.Item1)}, qFresh1);
            Edges.Add(edge1);
            AstToProgramGraph(ifElse.Item2, qFresh1, qAfterIfE);
            Node qFresh2 = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh2);
            Edge edge2 = new Edge(qBeforeIfE,
                new Condition() {Cond = MicroCTypes.booleanExpression.NewNot(ifElse.Item1), GraphvizSyntax = $"!({AstExtensions.AstToString(ifElse.Item1)})"}, qFresh2);
            Edges.Add(edge2);
            AstToProgramGraph(ifElse.Item3, qFresh2, qAfterIfE);
        }

        private void AstToProgramGraph(MicroCTypes.statement.While @while, Node qBeforeWhile, Node qAfterWhile)
        {
            Node qFresh = new Node(NodePrefix + Nodes.Count);
            Nodes.Add(qFresh);
            Edge edge1 = new Edge(qBeforeWhile,
                new Condition() {Cond = @while.Item1, GraphvizSyntax = AstExtensions.AstToString(@while.Item1)}, qFresh);
            Edges.Add(edge1);
            AstToProgramGraph(@while.Item2, qFresh, qBeforeWhile);
            Edge edge2 = new Edge(qBeforeWhile,
                new Condition() {Cond = MicroCTypes.booleanExpression.NewNot(@while.Item1), GraphvizSyntax = $"!({AstExtensions.AstToString(@while.Item1)})"},
                qAfterWhile);
            Edges.Add(edge2);
        }

    }
}