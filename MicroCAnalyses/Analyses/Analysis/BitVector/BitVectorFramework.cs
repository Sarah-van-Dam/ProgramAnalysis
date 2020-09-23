using System;
using System.Collections.Generic;
using Action = Analyses.Analysis.Actions;

namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework : Analysis
    {
        protected Operator joinOperator;
        protected Direction direction;
        protected HashSet<Constraint> constraints;

        protected abstract Constraint kill(Constraint constraint, Action action);

        protected abstract Constraint generate(Constraint constraint, Action action);

        public override void Analyse()
        {
            SolveConstraints();
        }

        private void SolveConstraints()
        {
            throw new NotImplementedException();
        }

    }
}