using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class ImageSourceBenchmarker
	{
		const int ImageCount = 100;

		readonly Image[] _explicitlySizedImages = new Image[ImageCount];
		readonly Image[] _autoSizedImages = new Image[ImageCount];
		readonly BenchmarkImageHandler[] _explicitHandlers = new BenchmarkImageHandler[ImageCount];
		readonly BenchmarkImageHandler[] _autoHandlers = new BenchmarkImageHandler[ImageCount];
		readonly ImageSource[,] _explicitSources = new ImageSource[2, ImageCount];
		readonly ImageSource[,] _autoSources = new ImageSource[2, ImageCount];

		int _sourceIndex;

		public ImageSourceBenchmarker()
		{
			for (int i = 0; i < ImageCount; i++)
			{
				_explicitlySizedImages[i] = new Image
				{
					WidthRequest = 20,
					HeightRequest = 20,
					Source = $"File{i}_0.png"
				};
				_explicitlySizedImages[i].Handler = _explicitHandlers[i] = new BenchmarkImageHandler();

				_autoSizedImages[i] = new Image
				{
					Source = $"File{i}_0.png"
				};
				_autoSizedImages[i].Handler = _autoHandlers[i] = new BenchmarkImageHandler();

				_explicitSources[0, i] = ImageSource.FromFile($"File{i}_0.png");
				_explicitSources[1, i] = ImageSource.FromFile($"File{i}_1.png");
				_autoSources[0, i] = ImageSource.FromFile($"File{i}_0.png");
				_autoSources[1, i] = ImageSource.FromFile($"File{i}_1.png");
			}
		}

		[Benchmark(Baseline = true)]
		public int SwapAutoSizedImageSources()
		{
			return SwapSources(_autoSizedImages, _autoSources, _autoHandlers);
		}

		[Benchmark]
		public int SwapExplicitlySizedImageSources()
		{
			return SwapSources(_explicitlySizedImages, _explicitSources, _explicitHandlers);
		}

		int SwapSources(Image[] images, ImageSource[,] sources, BenchmarkImageHandler[] handlers)
		{
			var sourceIndex = _sourceIndex++ & 1;

			for (int i = 0; i < images.Length; i++)
				images[i].Source = sources[sourceIndex, i];

			var invalidationCount = 0;
			for (int i = 0; i < handlers.Length; i++)
				invalidationCount += handlers[i].ConsumeInvalidateMeasureCount();

			return invalidationCount;
		}
	}

	[MemoryDiagnoser]
	public class ImageSourceLayoutBenchmarker
	{
		const int Columns = 10;
		const int ImageCount = 100;

		readonly Grid _explicitGrid;
		readonly Grid _autoGrid;
		readonly Image[] _explicitlySizedImages = new Image[ImageCount];
		readonly Image[] _autoSizedImages = new Image[ImageCount];
		readonly BenchmarkImageHandler[] _explicitHandlers = new BenchmarkImageHandler[ImageCount];
		readonly BenchmarkImageHandler[] _autoHandlers = new BenchmarkImageHandler[ImageCount];
		readonly ImageSource[,] _explicitSources = new ImageSource[2, ImageCount];
		readonly ImageSource[,] _autoSources = new ImageSource[2, ImageCount];

		int _layoutPasses;
		int _layoutChecksum;
		int _sourceIndex;

		public ImageSourceLayoutBenchmarker()
		{
			_explicitGrid = CreateImageGrid();
			_autoGrid = CreateImageGrid();

			for (int i = 0; i < ImageCount; i++)
			{
				var row = i / Columns;
				var column = i % Columns;

				_explicitlySizedImages[i] = new Image
				{
					WidthRequest = 20,
					HeightRequest = 20,
					Source = $"File{i}_0.png"
				};
				_explicitHandlers[i] = new BenchmarkImageHandler(() => RunLayout(_explicitGrid));
				_explicitlySizedImages[i].Handler = _explicitHandlers[i];
				Grid.SetRow(_explicitlySizedImages[i], row);
				Grid.SetColumn(_explicitlySizedImages[i], column);
				_explicitGrid.Add(_explicitlySizedImages[i]);

				_autoSizedImages[i] = new Image
				{
					Source = $"File{i}_0.png"
				};
				_autoHandlers[i] = new BenchmarkImageHandler(() => RunLayout(_autoGrid));
				_autoSizedImages[i].Handler = _autoHandlers[i];
				Grid.SetRow(_autoSizedImages[i], row);
				Grid.SetColumn(_autoSizedImages[i], column);
				_autoGrid.Add(_autoSizedImages[i]);

				_explicitSources[0, i] = ImageSource.FromFile($"File{i}_0.png");
				_explicitSources[1, i] = ImageSource.FromFile($"File{i}_1.png");
				_autoSources[0, i] = ImageSource.FromFile($"File{i}_0.png");
				_autoSources[1, i] = ImageSource.FromFile($"File{i}_1.png");
			}
		}

		[Benchmark(Baseline = true)]
		public int SwapAutoSizedImageSourcesWithLayoutInvalidation()
		{
			return SwapSources(_autoSizedImages, _autoSources, _autoHandlers);
		}

		[Benchmark]
		public int SwapExplicitlySizedImageSourcesWithLayoutInvalidation()
		{
			return SwapSources(_explicitlySizedImages, _explicitSources, _explicitHandlers);
		}

		static Grid CreateImageGrid()
		{
			var grid = new Grid
			{
				ColumnSpacing = 0,
				RowSpacing = 0
			};

			for (int i = 0; i < Columns; i++)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
				grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			}

			return grid;
		}

		void RunLayout(Grid grid)
		{
			_layoutPasses++;

			var measured = grid.CrossPlatformMeasure(500, 500);
			var arranged = grid.CrossPlatformArrange(new Rect(0, 0, 500, 500));

			_layoutChecksum = unchecked(_layoutChecksum +
				(int)measured.Width +
				(int)measured.Height +
				(int)arranged.Width +
				(int)arranged.Height);
		}

		int SwapSources(Image[] images, ImageSource[,] sources, BenchmarkImageHandler[] handlers)
		{
			_layoutPasses = 0;
			_layoutChecksum = 0;

			var sourceIndex = _sourceIndex++ & 1;

			for (int i = 0; i < images.Length; i++)
				images[i].Source = sources[sourceIndex, i];

			var invalidationCount = 0;
			for (int i = 0; i < handlers.Length; i++)
				invalidationCount += handlers[i].ConsumeInvalidateMeasureCount();

			return invalidationCount + _layoutPasses + _layoutChecksum;
		}
	}

	sealed class BenchmarkImageHandler : IViewHandler
	{
		readonly Action _invalidateLayout;
		readonly Size _measuredImageSize = new(20, 20);

		public BenchmarkImageHandler(Action invalidateLayout = null)
		{
			_invalidateLayout = invalidateLayout;
		}

		public bool HasContainer { get; set; }

		public object PlatformView { get; } = new object();

		public object ContainerView => null;

		public int InvalidateMeasureCount { get; private set; }

		public IView VirtualView { get; private set; }

		IElement IElementHandler.VirtualView => VirtualView;

		public IMauiContext MauiContext => null;

		public int ConsumeInvalidateMeasureCount()
		{
			var count = InvalidateMeasureCount;
			InvalidateMeasureCount = 0;
			return count;
		}

		public void DisconnectHandler()
		{
			VirtualView = null;
		}

		public Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (VirtualView is null)
				return _measuredImageSize;

			var width = Dimension.IsExplicitSet(VirtualView.Width) ? VirtualView.Width : _measuredImageSize.Width;
			var height = Dimension.IsExplicitSet(VirtualView.Height) ? VirtualView.Height : _measuredImageSize.Height;

			return new Size(width, height);
		}

		public void Invoke(string command, object args = null)
		{
			if (command == nameof(IView.InvalidateMeasure))
			{
				InvalidateMeasureCount++;
				_invalidateLayout?.Invoke();
			}
		}

		public void PlatformArrange(Rect frame)
		{
			if (VirtualView is not null)
				VirtualView.Frame = frame;
		}

		public void SetMauiContext(IMauiContext mauiContext)
		{
		}

		public void SetVirtualView(IElement view)
		{
			VirtualView = (IView)view;
		}

		public void UpdateValue(string property)
		{
		}
	}
}
