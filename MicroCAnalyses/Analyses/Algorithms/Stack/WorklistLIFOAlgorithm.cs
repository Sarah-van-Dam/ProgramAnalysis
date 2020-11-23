using Analyses.Graph;
using System.Collections.Generic;

namespace Analyses.Algorithms.Stack
{
	class WorklistLIFOAlgorithm : WorklistAlgorithm
	{
		public WorklistLIFOAlgorithm()
		{
			BasicActionsNeeded = 0;
		}
		
		private Stack<Node> stack = new Stack<Node>();

		public override bool Empty()
			=> stack.Count == 0;

		public override Node Extract()
			=> stack.Pop();

		public override void Insert(Node q)
		{
			stack.Push(q);
			BasicActionsNeeded++;
		}
	}
}
