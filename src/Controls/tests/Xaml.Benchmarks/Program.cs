using BenchmarkDotNet.Running;

namespace Microsoft.Maui.Controls.Xaml.Benchmarks;

class Program
{
	static void Main(string[] args)
	{		
		BenchmarkSwitcher.FromAssembly (typeof (Program).Assembly).Run (args);
	}
}
