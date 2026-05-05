using BenchmarkDotNet.Running;
using Microsoft.Maui.Benchmarks;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}