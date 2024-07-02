using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class FlexLayoutBenchmarker
	{
		readonly Border[] _views =
		[
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border(),
			new Border()
		];

		const int Iterations = 100;

		[GlobalSetup]
		public void Setup()
		{
			foreach (var view in _views)
			{
				view.WidthRequest = 100 * Random.Shared.NextDouble();
				view.HeightRequest = 100 * Random.Shared.NextDouble();
			}
		}

		[Benchmark]
		public void LayoutLotsOfItemsWithWrap()
		{
			var layout = new FlexLayout { Wrap = FlexWrap.Wrap };

			LayoutLotsOfItems(layout);
		}

		[Benchmark]
		public void LayoutLotsOfItemsNoWrap()
		{
			var layout = new FlexLayout { Wrap = FlexWrap.NoWrap };

			LayoutLotsOfItems(layout);
		}

		private void LayoutLotsOfItems(FlexLayout layout)
		{
			var parent = new Grid();

			layout.Parent = parent;

			for (int x = 0; x < Iterations; x++)
			{
				foreach (var view in _views)
				{
					layout.Add(view);

					// Vary the size of the layout and the views
					double layoutWidth = x * 10 * Random.Shared.NextDouble();
					double layoutHeight = x * 10 * Random.Shared.NextDouble();
					layout.WidthRequest = layoutWidth;
					layout.HeightRequest = layoutHeight;
					view.WidthRequest = x * Random.Shared.NextDouble();
					view.HeightRequest = x * Random.Shared.NextDouble();

					// Vary the properties of the views
					FlexLayout.SetBasis(view, (x % 2 == 0) ? FlexBasis.Auto : new FlexBasis(1, true));
					FlexLayout.SetOrder(view, x % 5);
					FlexLayout.SetGrow(view, x % 3);
					FlexLayout.SetShrink(view, x % 4);

					layout.Layout(new Rect(0, 0, layoutWidth, layoutHeight));

					// Remove every 10th view
					if (x % 10 == 0)
					{
						layout.Remove(view);
					}
				}
			}
		}
	}
}
