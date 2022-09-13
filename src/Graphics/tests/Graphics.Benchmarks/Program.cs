using BenchmarkDotNet.Running;

namespace Microsoft.Maui.Graphics.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}
