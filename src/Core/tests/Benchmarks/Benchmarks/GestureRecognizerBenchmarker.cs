using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks;

[MemoryDiagnoser]
public class GestureRecognizerBenchmarker
{
	View[] _views = null;
	IGestureRecognizer[] _gestureRecognizers = null;

	[GlobalSetup]
	public void Setup()
	{
		_views = [
			new Border(), new BoxView(), new CarouselView(), new Grid(), new Entry(), new Picker(), new CollectionView(),
			new CheckBox(), new DatePicker(), new Stepper(), new Slider(), new ActivityIndicator(),
			#pragma warning disable CS0618 // Type or member is obsolete
			new Frame(),
			#pragma warning restore CS0618 // Type or member is obsolete
			new ContentView(), new ProgressBar(), new SearchBar(), new Switch(), new TimePicker(), new WebView(), new Button(),
		];

		_gestureRecognizers = [
			new PointerGestureRecognizer(), new TapGestureRecognizer(), new PanGestureRecognizer(),
			new SwipeGestureRecognizer(), new DragGestureRecognizer(), new DropGestureRecognizer(),
		];
	}

	[Benchmark]
	public void AddLotsOfGestureRecognizers()
	{
		var layout = new VerticalStackLayout();

		AddGestureRecognizers(layout, _gestureRecognizers);

		for (int i = 0; i < 100; i++)
		{
			var childLayout = new VerticalStackLayout();

			AddGestureRecognizers(childLayout, _gestureRecognizers);

			foreach (var view in _views)
			{
				AddGestureRecognizers(view, _gestureRecognizers);

				childLayout.Add(view);
			}

			layout.Add(childLayout);
		}
	}


	[Benchmark]
	public void ClearLotsOfGestureRecognizers()
	{
		var layout = new VerticalStackLayout();

		for (int i = 0; i < 1000; i++)
		{
			AddGestureRecognizers(layout, _gestureRecognizers);
		}

		layout.GestureRecognizers.Clear();
	}


	[Benchmark]
	public void RemoveLotsOfGestureRecognizers()
	{
		var layout = new VerticalStackLayout();

		foreach (var gestureRecognizer in _gestureRecognizers)
		{
			for (int i = 0; i < 1000; i++)
			{
				layout.GestureRecognizers.Remove(gestureRecognizer);
			}
		}
	}

	[Benchmark]
	public void AddOneGestureRecognizer()
	{
		var gestureRecognizer = new PointerGestureRecognizer();

		for (int i = 0; i < 1000; i++)
		{
			var button = new Button();
			button.GestureRecognizers.Add(gestureRecognizer);
		}
	}

	[Benchmark]
	public void RemoveOneGestureRecognizer()
	{
		var gestureRecognizer = new PointerGestureRecognizer();

		for (int i = 0; i < 1000; i++)
		{
			var button = new Button();
			button.GestureRecognizers.Remove(gestureRecognizer);
		}
	}

	[Benchmark]
	public void ClearOneGestureRecognizer()
	{
		for (int i = 0; i < 1000; i++)
		{
			var button = new Button();
			var gestureRecognizer = new PointerGestureRecognizer();
			button.GestureRecognizers.Add(gestureRecognizer);
			button.GestureRecognizers.Clear();
		}
	}

	private static void AddGestureRecognizers(View view, params IGestureRecognizer[] gestureRecognizers)
	{
		foreach (var gestureRecognizer in gestureRecognizers)
		{
			view.GestureRecognizers.Add(gestureRecognizer);
		}
	}
}
