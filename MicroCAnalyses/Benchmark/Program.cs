using Analyses.Graph;
using System.Diagnostics;

namespace Benchmark
{
	class Program
	{
		static void Main(string[] args)
		{
			ProgramGraph longRecordSum = new ProgramGraph(
				Parser.parse(
					"{ int fst; int snd } r; int[5] a; int i; int j; int k; r := (1, 1); a[0] := 1; a[1] := 3; a[2] := 6; a[3] := 1; a[4] := 4; i := 0; read k; " +
					"if (k < 0) { k := k * -1; } if (k == 0) { k := 1; } while (i < k) { r.fst := r.fst + 1; r.fst := r.fst % 5; j := 0; while (j < r.fst) { " +
					"r.snd := r.snd * 2; j := j + a[r.fst]; } } write j.fst; write j.snd;"
				)
			);

			Debug.Print("Long Record Sum Program:");
			new Benchmark().Perform(longRecordSum);

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
