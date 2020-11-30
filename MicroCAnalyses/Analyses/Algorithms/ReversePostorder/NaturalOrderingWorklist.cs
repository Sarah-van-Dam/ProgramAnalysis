using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;

namespace Analyses.Algorithms.ReversePostorder
{
    public class NaturalOrderingWorklist : WorklistAlgorithm
    {
        private readonly HashSet<Edge> _depthFirstSpanningTree;
        private readonly Direction _direction;
        private readonly List<Node> _nodesNeedingVisit; //V in literature
        private readonly HashSet<Node> _nodesToReconsider; //P IN literature
        private readonly ProgramGraph _programGraph;
        private readonly Dictionary<Node, int> _reversePostOrder;

        public NaturalOrderingWorklist(ProgramGraph pg, Direction direction)
        {
            _programGraph = pg;
            _nodesNeedingVisit = new List<Node>();
            _nodesToReconsider = new HashSet<Node>();
            _direction = direction;
            (_depthFirstSpanningTree, _reversePostOrder) = ReversePostOrderWithATwist();
        }

        private (HashSet<Edge> depthFirstSpanningTree, Dictionary<Node, int> reversePostOrdering)
            ReversePostOrderWithATwist()
        {
            var depthFirstSpanningTree = new HashSet<Edge>();
            var reversePostOrdering = new Dictionary<Node, int>();
            var visited = new HashSet<Node>();
            var currentNumber = _programGraph.Nodes.Count;
            var up = new Dictionary<Node, int>();
            var ip = new Dictionary<Node, (int rp, int up)>();
            Dfs(visited, up, ip, ref currentNumber,
                _programGraph.Nodes.Single(n =>
                    n.Name == (_direction == Direction.Forward ? ProgramGraph.StartNode : ProgramGraph.EndNode)),
                depthFirstSpanningTree, reversePostOrdering);
            return (depthFirstSpanningTree, reversePostOrdering);
        }

        private void Dfs(ISet<Node> visited, IDictionary<Node, int> up, IDictionary<Node, (int rp, int up)> ip,
            ref int currentNumber,
            Node currentNode, ISet<Edge> depthFirstSpanningTree, IDictionary<Node, int> reversePostOrdering)
        {
            visited.Add(currentNode);
            up[currentNode] = currentNumber;

            var isForward = _direction == Direction.Forward;

            var edgesToIterate = isForward ? currentNode.OutGoingEdges : currentNode.InGoingEdges;

            foreach (var edge in edgesToIterate.Where(e => !visited.Contains(isForward ? e.ToNode : e.FromNode)))
            {
                depthFirstSpanningTree.Add(edge);
                Dfs(visited, up, ip, ref currentNumber, isForward ? edge.ToNode : edge.FromNode, depthFirstSpanningTree,
                    reversePostOrdering);
            }

            reversePostOrdering[currentNode] = currentNumber;
            currentNumber--;
            ip[currentNode] = (reversePostOrdering[currentNode], up[currentNode]);
        }

        private Dictionary<Node, HashSet<Node>> NaturalLoop()
        {
            var loops = new Dictionary<Node, HashSet<Node>>();
            foreach (var node in _nodesToReconsider) loops[node] = new HashSet<Node>();

            var isForward = _direction == Direction.Forward;

            foreach (var edge in _nodesToReconsider.SelectMany(n => isForward ? n.OutGoingEdges : n.InGoingEdges))
                if (isForward && _reversePostOrder[edge.ToNode] <= _reversePostOrder[edge.FromNode])
                {
                    var nodeToExtend = edge.ToNode;

                    var containsEntry = loops.TryGetValue(nodeToExtend, out _);
                    if (!containsEntry)
                        //Happens when iterating through members of the final loops
                        continue;

                    loops[nodeToExtend].Add(nodeToExtend);
                    Build(edge.FromNode, nodeToExtend, loops);
                }
                else if (!isForward && _reversePostOrder[edge.FromNode] <= _reversePostOrder[edge.ToNode])
                {
                    var nodeToExtend = edge.FromNode;

                    var containsEntry = loops.TryGetValue(nodeToExtend, out _);
                    if (!containsEntry)
                        //Happens when iterating through members of the final loops
                        continue;

                    loops[nodeToExtend].Add(nodeToExtend);
                    Build(edge.ToNode, nodeToExtend, loops);
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
                    Build(isForward ? ingoingEdge.FromNode : ingoingEdge.ToNode, to, loops);
            }
        }

        public override bool Empty()
        {
            return !_nodesNeedingVisit.Any() && !_nodesToReconsider.Any();
        }

        public override void Insert(Node q)
        {
            if (!_nodesNeedingVisit.Contains(q)) _nodesToReconsider.Add(q);
        }

        public override Node Extract()
        {
            BasicActionsNeeded++;

            if (!_nodesNeedingVisit.Any())
            {
                var naturalLoops = NaturalLoop();

                var noAncestorsInP = naturalLoops.Where(n => !HasAncestorInP(n.Key))
                    .Select(kvp => kvp.Key).ToList(); //S in literature

                var noAncestorsInNaturalLoops = naturalLoops
                    .Where(n => 
                        //!naturalLoops.Any(nl => nl.Value.Contains(n.Key)) && 
                        !HasAncestorInNaturalLoop(n.Key, naturalLoops))
                    .Select(kvp => kvp.Key).ToList();

                var remainingNodes = _nodesToReconsider.Except(noAncestorsInNaturalLoops).ToList(); // P' in literature

                var nodesInReversePostOrder = ReversePostOrder(noAncestorsInNaturalLoops);
                var nodeToReturn = nodesInReversePostOrder.First();
                nodesInReversePostOrder.RemoveAt(0);

                _nodesToReconsider.Clear();
                _nodesToReconsider.UnionWith(remainingNodes);
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

        private bool HasAncestorInNaturalLoop(Node node, IReadOnlyDictionary<Node,HashSet<Node>> naturalLoops)
        {
            if (_direction == Direction.Forward)
            {
                var edgeInDepthFirstSpanningTree =
                    _depthFirstSpanningTree.SingleOrDefault(e =>
                        e.ToNode.Equals(node)); // Will be null if node == start node
                if (edgeInDepthFirstSpanningTree == null) return false;

                return naturalLoops.Any(n => n.Value.Contains(edgeInDepthFirstSpanningTree.FromNode)) ||
                       HasAncestorInNaturalLoop(edgeInDepthFirstSpanningTree.FromNode, naturalLoops);
            }
            else
            {
                var edgeInDepthFirstSpanningTree =
                    _depthFirstSpanningTree.SingleOrDefault(e =>
                        e.FromNode.Equals(node)); // Will be null if node == end node
                if (edgeInDepthFirstSpanningTree == null) return false;

                return naturalLoops.Any( n => n.Value.Contains(edgeInDepthFirstSpanningTree.ToNode)) ||
                       HasAncestorInNaturalLoop(edgeInDepthFirstSpanningTree.ToNode, naturalLoops);
            }
        }

        private bool HasAncestorInP(Node node)
        {
            if (_direction == Direction.Forward)
            {
                var edgeInDepthFirstSpanningTree =
                    _depthFirstSpanningTree.SingleOrDefault(e =>
                        e.ToNode.Equals(node)); // Will be null if node == start node
                if (edgeInDepthFirstSpanningTree == null) return false;

                return _nodesToReconsider.Contains(edgeInDepthFirstSpanningTree.FromNode) ||
                       HasAncestorInP(edgeInDepthFirstSpanningTree.FromNode);
            }
            else
            {
                var edgeInDepthFirstSpanningTree =
                    _depthFirstSpanningTree.SingleOrDefault(e =>
                        e.FromNode.Equals(node)); // Will be null if node == end node
                if (edgeInDepthFirstSpanningTree == null) return false;

                return _nodesToReconsider.Contains(edgeInDepthFirstSpanningTree.ToNode) ||
                       HasAncestorInP(edgeInDepthFirstSpanningTree.ToNode);
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
            Dfs(visited, up, ip, ref currentNumber, _direction == Direction.Forward ? nodes.First() : nodes.Last(),
                depthFirstSpanningTree, reversePostOrdering);

            var sortedList = new List<Node>();
            foreach (var (key, value) in reversePostOrdering.Where(rpo => nodes.Contains(rpo.Key)))
            {
                var index = 0;
                var current = sortedList.FirstOrDefault();
                if (current == null)
                {
                    sortedList.Add(key);
                }
                else
                {
                    while (reversePostOrdering[current] < value && index < sortedList.Count - 1)
                    {
                        index++;
                        current = sortedList[index];
                    }

                    if (index == sortedList.Count - 2)
                        sortedList.Add(key);
                    else
                        sortedList.Insert(index, key);
                }
            }

            return sortedList;
        }
    }
}