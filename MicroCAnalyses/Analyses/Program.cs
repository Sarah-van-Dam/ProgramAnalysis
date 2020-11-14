using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using System;
using System.Linq.Expressions;
using Analyses.Algorithms;
using Analyses.Analysis;

namespace Analyses
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Program Analysis program!");
            Console.WriteLine("Please enter a program: ");
            var program = Console.ReadLine();
            var correctProgram = false;
            ProgramGraph pg = null;
            while (!correctProgram)
            {
                try
                {
                    var ast = Parser.parse(
                        "int y; int x; y :=1; x :=5; while (x > 1) { y := x * y; x := x - 1; } x := 0;");
                    pg = new ProgramGraph(ast);

                }
                catch (Exception e)
                {
                    Console.WriteLine("The program you entered was invalid.");
                    break;
                }

                correctProgram = true;
            }
            
            Console.WriteLine("Please choose which analysis you want by entering the number next to the analysis.");
            Console.WriteLine("1. Reaching Definitions");
            Console.WriteLine("2. Live Variables");
            Console.WriteLine("3. Faint Variables");
            Console.WriteLine("4. Detection of Signs");
            var temp = Console.ReadLine();
            AnalysisImplementation chosenAnalysis;
            
            switch (temp)
            {
                case "1":
                    chosenAnalysis = AnalysisImplementation.ReachingDefinitions;
                    break;
                case "2":
                    chosenAnalysis = AnalysisImplementation.LiveVariables;
                    break;
                case "3":
                    chosenAnalysis = AnalysisImplementation.FaintVariables;
                    break;
                case "4":
                    chosenAnalysis = AnalysisImplementation.DetectionOfSigns;
                    break;
                default:
                    Console.WriteLine("The number didn't match one of the analyses.");
                    break;
            }
            
            
            Console.WriteLine("Please choose which algorithm you want by entering the number next to the analysis.");
            Console.WriteLine("1. Sorted iteration algorithm");
            Console.WriteLine("2. Worklist algorithm using a stack");
            Console.WriteLine("3. Worklist algorithm using a queue");
            Console.WriteLine("4. Worklist algorithm using a depth first tree and postorder");
            temp = Console.ReadLine();
            WorklistImplementation chosenAlgorithm;
            
            switch (temp)
            {
                case "1":
                    chosenAlgorithm = WorklistImplementation.SortedIteration;
                    break;
                case "2":
                    chosenAlgorithm = WorklistImplementation.Lifo;
                    break;
                case "3":
                    chosenAlgorithm = WorklistImplementation.Fifo;
                    break;
                case "4":
                    chosenAlgorithm = WorklistImplementation.DepthFirstPostOrder;
                    break;
                default:
                    Console.WriteLine("The number didn't match one of the algorithms.");
                    break;
            }
            
            
            
            var rd = new ReachingDefinitions(pg);
            rd.Analyse();

            
            // debug print outs
            // switch (ast)
            // {
            //     case MicroCTypes.expressionTree.DS ds:
            //         var declarations = ds.Item1;
            //         var statements = ds.Item2;
            //         Console.WriteLine(statements);
            //         break;
            //     case MicroCTypes.expressionTree.S s:
            //         statements = s.Item;
            //         break;
            // }
        }
    }
}