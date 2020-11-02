using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analyses.Algorithms.Stack
{
	class WorklistLIFOAlgorithm : WorklistAlgorithm
	{
		private Stack<Node> stack;

		public override bool Empty()
			=> stack.Count == 0;

		public override Node Extract()
			=> stack.Pop();

		public override void Insert(Node q)
			=> stack.Push(q);

		/*
		 * TO-DO:
		 * Since FIFO and LIFO are deterministic,
		 * create a counter for it with a test
		 * to check that the counter never changes.
		 */
	}
}
