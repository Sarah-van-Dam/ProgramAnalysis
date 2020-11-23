using Analyses.Graph;
using System.Diagnostics;

namespace Benchmark
{
	class Program
	{
		static void Main(string[] args)
		{
			ProgramGraph addIntegers = new ProgramGraph(
				Parser.parse(
					"int a; int b; int c; a := 3; read b; c := a + b; write c;"
				)
			);

			Debug.Print("Add Integers Program:");
			new Benchmark().Perform(addIntegers);

			ProgramGraph fibonacci = new ProgramGraph(
				Parser.parse(
					"int f2; int input; int current; f1 := 1; f2 := 1; read input; if (input == 0 | input == 1 ){ current := 1; } " +
					"while (input > 1) { current := f1 + f2; f2 := f1; f1 := current; input := input - 1; } write current;"
				)
			);

			Debug.Print("Fibonacci Program:");
			new Benchmark().Perform(fibonacci);
		}
	}
}
