using System;
using Analyses.Algorithms;
using Analyses.Algorithms.Stack;
using Analyses.Graph;

namespace Analyses.Analysis
{
    public abstract class Analysis
    {
        protected readonly ProgramGraph _program;
        public readonly WorklistAlgorithm _worklistAlgorithm;
        protected readonly Direction Direction;

        public Analysis(ProgramGraph programGraph, Direction direction,
            WorklistImplementation worklistImplementation = WorklistImplementation.SortedIteration)
        {
            _program = programGraph;
            Direction = direction;
            _worklistAlgorithm = worklistImplementation switch
            {
                WorklistImplementation.SortedIteration => new SortedIterationWorklist(Direction),
                WorklistImplementation.Lifo => new WorklistLIFOAlgorithm(),
                WorklistImplementation.Fifo => throw new NotImplementedException("Fifo worklist not implemented"),
                _ => throw new ArgumentOutOfRangeException(nameof(worklistImplementation), worklistImplementation, null)
            };
        }
        
        
        // ToDo: We need to figure out the return type the analyse method.
        public abstract void Analyse();
        
        
    }
}