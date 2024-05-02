using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class LayoutBenchmarker
	{
		static readonly Border[] Views = new[]
		{
			new Border(), new Border(), new Border(), new Border(), new Border(), new Border(), new Border(),
			new Border(), new Border(), new Border(), new Border(), new Border(), new Border(), new Border()
		};

		static readonly int Iterations = Views.Length;

		[Benchmark]
		public void GetLayoutHandlerIndex()
		{
			var layout = new VerticalStackLayout();

			for (int i = 0; i < Iterations; i++)
			{
				var view = Views[i];
				layout.Add(view);
				layout.GetLayoutHandlerIndex(view);
			}
		}
	}
}
