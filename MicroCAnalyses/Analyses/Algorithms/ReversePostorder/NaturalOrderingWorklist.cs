using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Analyses.Graph;

namespace Analyses.Algorithms.ReversePostorder
{
    public class NaturalOrderingWorklist : WorklistAlgorithm
    {
        private readonly ProgramGraph _programGraph;
        private readonly Dictionary<Node, int> _reversePostOrder;
        private readonly List<Node> _nodesNeedingVisit; //V in literature
        private readonly List<Node> _nodesToReconsider; //P IN literature

        public NaturalOrderingWorklist(ProgramGraph pg)
        {
            _programGraph = pg;
            _reversePostOrder = ReversePostOrderWithATwist();
            _nodesNeedingVisit = new List<Node>();
            _nodesToReconsider = new List<Node>();
        }

        private Dictionary<Node, int> ReversePostOrderWithATwist()
        {
            var depthFirstSpanningTree = new HashSet<Edge>();
            var reversePostOrdering = new Dictionary<Node, int>();
            var visited = new HashSet<Node>();
            var currentNumber = _programGraph.Nodes.Count;
            var up = new Dictionary<Node, int>();
            var ip = new Dictionary<Node, (int rp, int up)>();
            DFS(visited,up,ip, ref currentNumber, _programGraph.Nodes.Single(n => n.Name == ProgramGraph.StartNode), depthFirstSpanningTree, reversePostOrdering);
            return reversePostOrdering;
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
            foreach (var node in _nodesToReconsider)
            {
                loops[node] = new HashSet<Node>();
            }
            //TODO: For duality add direction and depending of that choose either outgoing or ingoing 
            foreach (var edge in _nodesToReconsider.SelectMany(n => n.OutGoingEdges)
                .Where(e => _reversePostOrder[e.ToNode] >= _reversePostOrder[e.FromNode]))
            {
                var containsEntry = loops.TryGetValue(edge.ToNode, out _);
                if (!containsEntry)
                {
                    //Happens when iterating through members of the final loops
                    continue;
                }
                loops[edge.ToNode].Add(edge.ToNode);
                Build(edge.FromNode, edge.ToNode, loops);
            }

            return loops;
        }


        private void Build(Node from, Node to, IReadOnlyDictionary<Node, HashSet<Node>> loops)
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
        }

        public override bool Empty()
        {
            return !_nodesNeedingVisit.Any() && !_nodesToReconsider.Any();
        }

        public override void Insert(Node q)
        {
            if (!_nodesNeedingVisit.Contains(q))
            {
                _nodesToReconsider.Add(q);
            }
        }

        public override Node Extract()
        {
            BasicActionsNeeded++;

            if (!_nodesNeedingVisit.Any())
            {
                var naturalLoops = NaturalLoop();
                var nodesInLoop = naturalLoops.Where(n => n.Value.Any()).SelectMany(n => n.Value).ToHashSet();
                var nodesWithNoPInLoopAncestors = //S in literature
                    naturalLoops.Where(s => !nodesInLoop.Contains(s.Key))
                        .Select(kvp => kvp.Key).ToList();
                var remainingNodes = _nodesToReconsider.Except(nodesWithNoPInLoopAncestors).ToList(); // P' in literature

                if (!nodesWithNoPInLoopAncestors.Any())
                {
                    var remainingNodesInReversePostOrder = ReversePostOrder(remainingNodes);
                    var remainingNodeToReturn = remainingNodesInReversePostOrder.First();
                    remainingNodesInReversePostOrder.RemoveAt(0);
                    _nodesToReconsider.Clear();
                    _nodesNeedingVisit.AddRange(remainingNodesInReversePostOrder);
                    return remainingNodeToReturn;
                }

                var nodesInReversePostOrder = ReversePostOrder(nodesWithNoPInLoopAncestors);
                var nodeToReturn = nodesInReversePostOrder.First();
                nodesInReversePostOrder.RemoveAt(0);

                _nodesToReconsider.Clear();
                _nodesToReconsider.AddRange(remainingNodes);
                _nodesNeedingVisit.AddRange(nodesInReversePostOrder);
                return nodeToReturn;
            }
            else
            {
                var nodeToReturn = _nodesNeedingVisit.First();
                _nodesNeedingVisit.RemoveAt(0);
                return nodeToReturn;
            }
        }

     
        private List<Node> ReversePostOrder(List<Node> nodes)
        {
            var depthFirstSpanningTree = new HashSet<Edge>();
            var reversePostOrdering = new Dictionary<Node, int>();
            var visited = new HashSet<Node>();
            var currentNumber = nodes.Count;
            var up = new Dictionary<Node, int>();
            var ip = new Dictionary<Node, (int rp, int up)>();
            DFS(visited, up, ip, ref currentNumber, nodes.First(), depthFirstSpanningTree, reversePostOrdering);

            var sortedList = new List<Node>();
            foreach (var kvp in reversePostOrdering.Where(rpo => nodes.Contains(rpo.Key)))
            {
                var index = 0;
                var current = sortedList.FirstOrDefault();
                if (current == null)
                {
                    sortedList.Add(kvp.Key);
                }
                else
                {
                    while (reversePostOrdering[current] > kvp.Value && index < sortedList.Count-1)
                    {
                        index++;
                        current = sortedList[index];
                    }

                    if (index == sortedList.Count - 1)
                    {
                        sortedList.Add(kvp.Key);
                    }
                    else
                    {
                        sortedList.Insert(index,kvp.Key);
                    }
                }
                
            }

            return sortedList;
        }
    }
}