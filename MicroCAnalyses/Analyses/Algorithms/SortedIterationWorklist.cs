using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Graph;
using Analyses.Helpers;

namespace Analyses.Algorithms
{
    public class SortedIterationWorklist : WorklistAlgorithm
    {
        private readonly List<Node> _nodes;
        private readonly Direction _direction;

        public SortedIterationWorklist(Direction direction)
        {
            _nodes = new List<Node>();
            _direction = direction;
        }
        
        public override bool Empty()
        {
            return !_nodes.Any();
        }

        public override void Insert(Node q)
        {
            _nodes.Add(q);
            SortingHelper.OrderNodesByDirection(_nodes, _direction == Direction.Forward);
        }

        public override Node Extract()
        {
            if (Empty())
            {
                throw new Exception("Worklist is empty. Cannot extract node");
            }

            var node = _nodes.First();
            _nodes.RemoveAt(0);
            return node;
        }
    }
}