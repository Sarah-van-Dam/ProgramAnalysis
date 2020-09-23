using System;

namespace Analyses
{
    class Program
    {
        static void Main(string[] args)
        {
            var test =  Parser.parse("int x; x:=1;");

            switch (test)
            {
                case MicroCTypes.expr.DS ds:
                    var declarations = ds.Item1;
                    var statements = ds.Item2;
                    Console.WriteLine(statements);
                    break;
                case MicroCTypes.expr.S s:
                    statements = s.Item;
                    break;
            }
            
            Console.WriteLine("Hello World!");
        }
    }
}