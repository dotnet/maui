using BenchmarkDotNet.Running;

namespace Xamarin.Platform.Handlers.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined();
		}
	}
}
