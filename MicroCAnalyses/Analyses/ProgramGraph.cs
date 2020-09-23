using System.Collections.Generic;

namespace Analyses
{
    public class ProgramGraph
    {
        public HashSet<Node> Nodes { get; private set; }
        public HashSet<Edge> Edges { get; private set; }

        public ProgramGraph(MicroCTypes.expr ast)
        {
            AstToProgramGraph(ast);
        }

        private void AstToProgramGraph(MicroCTypes.expr ast)
        {
            throw new System.NotImplementedException();
        }
    }
}