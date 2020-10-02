using Analyses.Graph;
using System.Collections.Generic;

namespace Analyses
{
    //TODO: make this back into interface
    public abstract class IConstraints
    {
        //TODO: this shouldn't live in the abstract class 
        public readonly Dictionary<string, HashSet<(string action, string startNode, string endNode)>>
    VariableToPossibleAssignments;

        /// <summary>
        /// True if current object is subset of <paramref name="other"/>:
        ///     are the keys the same  - are the tuples in this also contained in other
        ///     for all keys - are the tuples in this also contained in other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool IsSubset(IConstraints other);
  
    }
}