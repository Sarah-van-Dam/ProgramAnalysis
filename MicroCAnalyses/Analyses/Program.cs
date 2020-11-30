using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using System;
using System.IO;
using Analyses.Algorithms;
using Analyses.Analysis;
using Analyses.Analysis.BitVector.LiveVariablesAnalysis;
using Analyses.Analysis.Distributive;
using Analyses.Analysis.Monotone.DetectionOfSignsAnalysis;
using Analyses.Analysis.BitVector.AvailableExpressionAnalysis;

namespace Analyses
{
    class Program
    {
        private const string Algorithm = "algorithm";
        private const string Analysis = "analysis";
        private const string RunLiteral = "run";
        private const string Exit = "exit";
        private const string Help = "help";
        private const string Reload = "Reload";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Program Analysis program!");
            var pg = LoadProgramGraph();

            var chosenAnalysis = SetAnalysisType();


            var chosenAlgorithm = SetAlgorithm();

            Run(chosenAnalysis, pg, chosenAlgorithm);

            PrintHelp();
            
            var shouldRun = true;
            while (shouldRun)
            {
                switch (Console.ReadLine()?.ToLower())
                {
                    case Algorithm:
                        SetAlgorithm();
                        break;
                    case Analysis:
                        SetAnalysisType();
                        break;
                    case RunLiteral:
                        Run(chosenAnalysis, pg, chosenAlgorithm);
                        break;
                    case Exit:
                        shouldRun = false;
                        break;
                    case Help:
                        PrintHelp();
                        break;
                    case Reload:
                        pg = LoadProgramGraph();
                        break;
                    default:
                        Console.WriteLine("Sorry, I didn't understand your input. Try again or write \"help\" to get a list of commands.");
                        break;
                }
                
            }
        }

        private static ProgramGraph LoadProgramGraph()
        {
            Console.WriteLine("Please enter a program: ");
            // TODO read from file
            var correctProgram = false;
            ProgramGraph pg = null;
            const string programFileName = "program.mc";
            if (!File.Exists(programFileName))
            {
                File.Create(programFileName);
            }
            while (!correctProgram)
            {
                MicroCTypes.expressionTree ast;
                try
                {
                    string program;
                    
                    using (var file = File.Open(programFileName, FileMode.Open, FileAccess.Read))
                    using(var reader = new StreamReader(file))
                    {
                        program = reader.ReadToEnd();
                        
                    }
                    ast = Parser.parse(
                        program);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"The program you entered was invalid. Change the file {programFileName} and press enter");
                    Console.Read();
                    continue;
                }
                pg = new ProgramGraph(ast);
                correctProgram = true;
            }

            return pg;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Options for program: \n" +
                              $"\"{Algorithm}\": set a new algorithm \n" +
                              $"\"{Analysis}\": set a new analysis type \n" +
                              $"\"{RunLiteral}\": runs the analysis with the chosen result and print it in the console\n" +
                              $"\"{Exit}\": exits the analysis program\n" +
                              $"\"{Help}\": shows this information again.");
        }

        private static void Run(AnalysisImplementation chosenAnalysis, ProgramGraph pg, WorklistImplementation chosenAlgorithm)
        {
            Analysis.Analysis analysis = chosenAnalysis switch
            {
                AnalysisImplementation.ReachingDefinitions => new ReachingDefinitions(pg, chosenAlgorithm),
                AnalysisImplementation.LiveVariables => new LiveVariables(pg, chosenAlgorithm),
                AnalysisImplementation.FaintVariables => new FaintVariables(pg, chosenAlgorithm),
                AnalysisImplementation.DetectionOfSigns => new DetectionOfSigns(pg, chosenAlgorithm),
                AnalysisImplementation.DangerousVariables => new DangerousVariables(pg, chosenAlgorithm),
                AnalysisImplementation.AvailableExpressions => new AvailableExpressions(pg, chosenAlgorithm),
                _ => throw new ArgumentOutOfRangeException()
            };

            analysis.Analyse();
            analysis.PrintResult();
        }

        private static WorklistImplementation SetAlgorithm()
        {
            Console.WriteLine("Please choose which algorithm you want by entering the number next to the analysis.");
            Console.WriteLine("1. Sorted iteration algorithm");
            Console.WriteLine("2. Worklist algorithm using a stack");
            Console.WriteLine("3. Worklist algorithm using a queue");
            Console.WriteLine("4. Worklist algorithm using a depth first spanning tree, reverse post-order, and natural loops");
            var algorithmName = Console.ReadLine();

            var chosenAlgorithm = ChooseAlgorithm(algorithmName);
            return chosenAlgorithm;
        }

        private static AnalysisImplementation SetAnalysisType()
        {
            Console.WriteLine("Please choose which analysis you want by entering the number next to the analysis.");
            Console.WriteLine("1. Reaching Definitions");
            Console.WriteLine("2. Live Variables");
            Console.WriteLine("3. Faint Variables");
            Console.WriteLine("4. Detection of Signs");
            Console.WriteLine("5. Dangerous Variables");
            Console.WriteLine("6. Available Expressions");
            var analysisName = Console.ReadLine();

            var chosenAnalysis = ChooseAnalysis(analysisName);
            return chosenAnalysis;
        }

        private static WorklistImplementation ChooseAlgorithm(string algorithmName)
        {
            WorklistImplementation chosenAlgorithm;
            switch (algorithmName)
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
                    chosenAlgorithm = WorklistImplementation.NaturalLoops;
                    break;
                default:
                    Console.WriteLine("The number didn't match one of the algorithms.");
                    throw new Exception();
            }

            return chosenAlgorithm;
        }

        private static AnalysisImplementation ChooseAnalysis(string analysisName)
        {
            AnalysisImplementation chosenAnalysis;
            switch (analysisName)
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
                case "5":
                    chosenAnalysis = AnalysisImplementation.DangerousVariables;
                    break;
                case "6":
                    chosenAnalysis = AnalysisImplementation.AvailableExpressions;
                    break;
                default:
                    Console.WriteLine("The number didn't match one of the analyses.");
                    throw new Exception();
            }

            return chosenAnalysis;
        }
    }
}