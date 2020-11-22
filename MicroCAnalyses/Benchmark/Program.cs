using Analyses.Graph;

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

			new Benchmark().Perform(addIntegers);
		}
	}
}
