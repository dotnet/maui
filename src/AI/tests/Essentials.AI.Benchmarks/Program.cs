using BenchmarkDotNet.Running;

namespace Microsoft.Maui.Essentials.AI.Benchmarks;

class Program
{
	static void Main(string[] args)
	{
		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
	}
}
