using Analyses.Graph;
using System.Collections.Generic;

namespace Analyses
{
    public abstract class Constraints
    {
        public readonly Dictionary<string, HashSet<Constraints>>
    VariableToPossibleAssignments;

    }
}