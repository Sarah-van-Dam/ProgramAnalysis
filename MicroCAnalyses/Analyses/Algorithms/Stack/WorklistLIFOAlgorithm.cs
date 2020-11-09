using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analyses.Algorithms.Stack
{
	class WorklistLIFOAlgorithm : WorklistAlgorithm
	{
		private Stack<Node> stack = new Stack<Node>();

		/// <summary>
		/// The amount of times <see cref="Insert(Node)"/> has been called.
		/// </summary>
		public ulong Counter { get; private set; }

		public override bool Empty()
			=> stack.Count == 0;

		public override Node Extract()
			=> stack.Pop();

		public override void Insert(Node q)
		{
			stack.Push(q);
			Counter++;
		}

		/*
		 * TO-DO:
		 * Since FIFO and LIFO are deterministic,
		 * create a counter for it with a test
		 * to check that the counter never changes.
		 */
	}
}
