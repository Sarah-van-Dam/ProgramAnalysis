using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;

namespace Analyses.Algorithms.ReversePostorder
{
    public class NaturalOrderingWorklist : WorklistAlgorithm
    {
        private readonly ProgramGraph _programGraph;
        private readonly Dictionary<Node, int> _reversePostOrder;
        private readonly List<Node> _nodesNeedingVisit; //V in literature
        private readonly List<Node> _nodesToReconsider; //P IN literature
        private readonly Direction _direction;

        public NaturalOrderingWorklist(ProgramGraph pg, Direction direction)
        {
            _programGraph = pg;
            _nodesNeedingVisit = new List<Node>();
            _nodesToReconsider = new List<Node>();
            _direction = direction;
            _reversePostOrder = ReversePostOrderWithATwist();
        }

        private Dictionary<Node, int> ReversePostOrderWithATwist()
        {
            var depthFirstSpanningTree = new HashSet<Edge>();
            var reversePostOrdering = new Dictionary<Node, int>();
            var visited = new HashSet<Node>();
            var currentNumber = _programGraph.Nodes.Count;
            var up = new Dictionary<Node, int>();
            var ip = new Dictionary<Node, (int rp, int up)>();
            DFS(visited,up,ip, ref currentNumber, _programGraph.Nodes.Single(n => n.Name == (_direction == Direction.Forward ? ProgramGraph.StartNode : ProgramGraph.EndNode)), depthFirstSpanningTree, reversePostOrdering);
            return reversePostOrdering;
        }

        private void DFS(HashSet<Node> visited, Dictionary<Node,int> up, Dictionary<Node,(int rp, int up)> ip, ref int currentNumber, Node currentNode, HashSet<Edge> depthFirstSpanningTree, Dictionary<Node, int> reversePostOrdering)
        {
            visited.Add(currentNode);
            up[currentNode] = currentNumber;

            var isForward = _direction == Direction.Forward;

            var edgesToIterate = isForward ? currentNode.OutGoingEdges : currentNode.InGoingEdges;

            foreach (var edge in edgesToIterate.Where(e => !visited.Contains(isForward ? e.ToNode : e.FromNode)))
            {
                depthFirstSpanningTree.Add(edge);
                DFS(visited,up,ip,ref currentNumber, isForward ? edge.ToNode : edge.FromNode,depthFirstSpanningTree,reversePostOrdering);
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
            var isForward = _direction == Direction.Forward;

            var rpoComparison = isForward
                ? new System.Func<Edge, bool>(e => _reversePostOrder[e.ToNode] >= _reversePostOrder[e.FromNode])
                : (e => _reversePostOrder[e.FromNode] >= _reversePostOrder[e.ToNode]); 

            foreach (var edge in _nodesToReconsider.SelectMany(n => isForward ? n.OutGoingEdges : n.InGoingEdges)
                .Where(rpoComparison))
            {
                var nodeToExtend = isForward ? edge.ToNode : edge.FromNode;

                var containsEntry = loops.TryGetValue(nodeToExtend, out _);
                if (!containsEntry)
                {
                    //Happens when iterating through members of the final loops
                    continue;
                }
                
                loops[nodeToExtend].Add(nodeToExtend);
                Build(isForward ? edge.FromNode : edge.ToNode, nodeToExtend, loops);
            }

            return loops;
        }


        private void Build(Node from, Node to, IReadOnlyDictionary<Node, HashSet<Node>> loops)
        {
            var isForward = _direction == Direction.Forward;
            
            if (_reversePostOrder[to] <= _reversePostOrder[from]
                && !loops[to].Contains(from))
            {
                loops[to].Add(from);
                foreach (var ingoingEdge in isForward ? from.InGoingEdges : from.OutGoingEdges)
                {
                    Build(isForward ? ingoingEdge.FromNode : ingoingEdge.ToNode, to, loops);
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
            DFS(visited, up, ip, ref currentNumber, _direction == Direction.Forward ? nodes.First() : nodes.Last(), depthFirstSpanningTree, reversePostOrdering);

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
                    while (reversePostOrdering[current] < kvp.Value && index < sortedList.Count-1)
                    {
                        index++;
                        current = sortedList[index];
                    }

                    if (index == sortedList.Count - 2)
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