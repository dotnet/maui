using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class VisualTreeBenchmarker
	{
		static readonly View[] Views = [
			new Border(),
			new BoxView(),
			new CarouselView(),
			new Grid(),
			new Entry(),
			new Picker(),
			new CollectionView(),
			new CheckBox(),
			new DatePicker(),
			new Stepper(),
			new Slider(),
			new ActivityIndicator(),
			new Frame(),
			new ContentView(),
			new ProgressBar(),
			new SearchBar(),
			new Switch(),
			new TimePicker(),
			new WebView(),
			new Button(),
		];

		private const int Iterations = 100;

		[Benchmark]
		public void GetVisualTreeElements()
		{
			var layout = new VerticalStackLayout();

			for (int i = 0; i < Iterations; i++)
			{
				var childLayout = new VerticalStackLayout();

				foreach (var view in Views)
				{
					childLayout.Add(view);

					var grandchildLayout = new VerticalStackLayout();

					foreach (var view2 in Views)
					{
						grandchildLayout.Add(view);
						grandchildLayout.GetVisualTreeElements(grandchildLayout.Frame);
					}

					layout.Add(grandchildLayout);

					childLayout.GetVisualTreeElements(childLayout.Frame);
				}

				layout.Add(childLayout);

				layout.GetVisualTreeElements(layout.Frame);
			}
		}
	}
}
