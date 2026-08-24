using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class LayoutExtensionsBenchmarker
	{
		const int Iterations = 1_000;

		readonly Rect _bounds = new Rect(0, 0, 300, 300);
		StubBase _centerFits;
		StubBase _centerOverflows;
		StubBase _endOverflows;
		StubBase _fillFits;

		[GlobalSetup]
		public void Setup()
		{
			_fillFits = CreateView(
				new Thickness(0),
				new Size(100, 100),
				LayoutAlignment.Fill,
				LayoutAlignment.Fill);
			_centerFits = CreateView(
				new Thickness(10),
				new Size(120, 120),
				LayoutAlignment.Center,
				LayoutAlignment.Center);
			_centerOverflows = CreateView(
				new Thickness(200),
				new Size(500, 500),
				LayoutAlignment.Center,
				LayoutAlignment.Center);
			_endOverflows = CreateView(
				new Thickness(200),
				new Size(500, 500),
				LayoutAlignment.End,
				LayoutAlignment.End);
		}

		[Benchmark(OperationsPerInvoke = Iterations)]
		public Rect FillFits() => ComputeFrames(_fillFits);

		[Benchmark(OperationsPerInvoke = Iterations)]
		public Rect CenterFits() => ComputeFrames(_centerFits);

		[Benchmark(OperationsPerInvoke = Iterations)]
		public Rect CenterOverflows() => ComputeFrames(_centerOverflows);

		[Benchmark(OperationsPerInvoke = Iterations)]
		public Rect EndOverflows() => ComputeFrames(_endOverflows);

		Rect ComputeFrames(StubBase view)
		{
			var result = default(Rect);
			for (var iteration = 0; iteration < Iterations; iteration++)
				result = view.ComputeFrame(_bounds);

			return result;
		}

		static StubBase CreateView(
			Thickness margin,
			Size desiredSize,
			LayoutAlignment horizontalAlignment,
			LayoutAlignment verticalAlignment)
		{
			return new StubBase
			{
				Margin = margin,
				DesiredSize = desiredSize,
				HorizontalLayoutAlignment = horizontalAlignment,
				VerticalLayoutAlignment = verticalAlignment,
				Width = Dimension.Unset,
				Height = Dimension.Unset,
				MaximumWidth = Dimension.Maximum,
				MaximumHeight = Dimension.Maximum,
			};
		}
	}
}
