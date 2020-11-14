using Analyses.Graph;
using System.Collections.Generic;

namespace Analyses.Algorithms.Queue
{
    public class WorklistFIFOAlgorithm : WorklistAlgorithm
    {
        private Queue<Node> queue = new Queue<Node>();


        public override bool Empty()
        {
            return queue.Count == 0;
        }

        public override void Insert(Node q)
        {
            queue.Enqueue(q);
            BasicActionsNeeded++;
        }

        public override Node Extract()
        {
            return queue.Dequeue();
            
        }
    }
}