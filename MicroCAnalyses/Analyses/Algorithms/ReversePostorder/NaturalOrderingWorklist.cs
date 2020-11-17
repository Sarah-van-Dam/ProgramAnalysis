using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;

namespace Analyses.Algorithms.ReversePostorder
{
    public class NaturalOrderingWorklist : WorklistAlgorithm
    {
        private readonly ProgramGraph _programGraph;
        private Dictionary<Node, int> _reversePostOrder;
        private HashSet<Edge> _depthFirstSpanningTree;

        public NaturalOrderingWorklist(ProgramGraph pg)
        {
            _programGraph = pg;
            (_depthFirstSpanningTree, _reversePostOrder) = ReversePostOrderWithATwist();
            var loops = NaturalLoop();
        }

        // private void InitializeAlgorithm()
        // {
        //     var dps = DepthFirstSpanningTree.CreateDepthFirstSpanningTreeFromProgramGraph(_programGraph);
        //
        //     var travelOrder = PostOrderTraversal();
        //     travelOrder.Reverse();
        //     _reversePostOrder = travelOrder;
        // }

        private (HashSet<Edge> depthFirstSpanningTree, Dictionary<Node, int> reversePostOrdering) ReversePostOrderWithATwist()
        {
            var depthFirstSpanningTree = new HashSet<Edge>();
            var reversePostOrdering = new Dictionary<Node, int>();
            var visited = new HashSet<Node>();
            var currentNumber = _programGraph.Nodes.Count;
            var up = new Dictionary<Node, int>();
            var ip = new Dictionary<Node, (int rp, int up)>();
            DFS(visited,up,ip, ref currentNumber, _programGraph.Nodes.Single(n => n.Name == ProgramGraph.StartNode), depthFirstSpanningTree, reversePostOrdering);
            return (depthFirstSpanningTree, reversePostOrdering);
        }

        private static void DFS(HashSet<Node> visited, Dictionary<Node,int> up, Dictionary<Node,(int rp, int up)> ip, ref int currentNumber, Node currentNode, HashSet<Edge> depthFirstSpanningTree, Dictionary<Node, int> reversePostOrdering)
        {
            visited.Add(currentNode);
            up[currentNode] = currentNumber;
            foreach (var edge in currentNode.OutGoingEdges.Where(e => !visited.Contains(e.ToNode)))
            {
                depthFirstSpanningTree.Add(edge);
                DFS(visited,up,ip,ref currentNumber,edge.ToNode,depthFirstSpanningTree,reversePostOrdering);
            }
            reversePostOrdering[currentNode] = currentNumber;
            currentNumber--;
            ip[currentNode] = (reversePostOrdering[currentNode], up[currentNode]);
        }

        private Dictionary<Node, HashSet<Node>> NaturalLoop()
        {
            var loops = new Dictionary<Node, HashSet<Node>>();
            foreach (var node in _programGraph.Nodes)
            {
                loops[node] = new HashSet<Node>();
            }

            foreach (var edge in _programGraph.Edges
                .Where(e => _reversePostOrder[e.ToNode] <= _reversePostOrder[e.FromNode]))
            {
                loops[edge.ToNode].Add(edge.ToNode);
                Build(edge.FromNode, edge.ToNode, loops);
            }

            return loops;
        }

        private void Build(Node from, Node to, Dictionary<Node,HashSet<Node>> loops)
        {
            if (_reversePostOrder[to] <= _reversePostOrder[from]
                && !loops[to].Contains(from))
            {
                loops[to].Add(from);
                foreach (var ingoingEdge in from.InGoingEdges)
                {
                    Build(ingoingEdge.FromNode, to, loops);
                }
            }
            //TODO: Figure out why. Is this needed? Do we need to rollback something?
            //else if (_reversePostOrder[edge.ToNode] > _reversePostOrder[edge.FromNode])
            //{ 
                //throw new Exception("Abort");
            //}
        }

        private List<Node> PostOrderTraversal()
        {
            return null;
        }

        public override bool Empty()
        {
            throw new System.NotImplementedException();
        }

        public override void Insert(Node q)
        {
            throw new System.NotImplementedException();
        }

        public override Node Extract()
        {
            throw new System.NotImplementedException();
        }
    }
}