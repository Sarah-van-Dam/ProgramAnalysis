using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using System;

namespace Analyses
{
    class Program
    {
        static void Main(string[] args)
        {
            var ast =  Parser.parse("int y; y :=1; while (x > 0) { y := x * y; x := x - 1; }");
            var pg = new ProgramGraph(ast);
            var rd = new ReachingDefinitions(pg);
            rd.Analyse();

            
            // debug print outs
            switch (ast)
            {
                case MicroCTypes.expressionTree.DS ds:
                    var declarations = ds.Item1;
                    var statements = ds.Item2;
                    Console.WriteLine(statements);
                    break;
                case MicroCTypes.expressionTree.S s:
                    statements = s.Item;
                    break;
            }
            
            Console.WriteLine("Hello World!");
        }
    }
}