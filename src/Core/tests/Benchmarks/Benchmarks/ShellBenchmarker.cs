using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class ShellBenchmarker
	{
		Shell shell;

		[GlobalSetup]
		public void Setup()
		{
			shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var section1 = new ShellSection { Route = "tabone", Items = { new ShellContent { Route = "content" } } };
			one.Items.Add(section1);
			var section2 = new ShellSection { Route = "tabtwo", Items = { new ShellContent { Route = "content" } } };
			two.Items.Add(section2);

			shell.Items.Add(one);
			shell.Items.Add(two);
		}

		[Benchmark]
		public async Task GoTo()
		{
			await shell.GoToAsync(new ShellNavigationState("//two/tabtwo/content"));
		}
	}
}
