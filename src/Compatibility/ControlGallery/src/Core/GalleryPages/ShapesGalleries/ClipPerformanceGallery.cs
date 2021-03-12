using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.ShapesGalleries
{
	public class ClipPerformanceGallery : ContentPage
	{
		readonly Stopwatch _stopwatch;

		public ClipPerformanceGallery()
		{
			Title = "Clip Performance Gallery";

			_stopwatch = new Stopwatch();

			var layout = new StackLayout();

			var imageButton = new Button
			{
				Text = "Measure Image"
			};

			var clipImageButton = new Button
			{
				Text = "Measure Clip Image"
			};

			layout.Children.Add(imageButton);
			layout.Children.Add(clipImageButton);

			Content = layout;

			imageButton.Clicked += (sender, args) =>
			{
				_stopwatch.Start();
				Navigation.PushAsync(new ImagesPerformanceTestPage(_stopwatch));
			};

			clipImageButton.Clicked += (sender, args) =>
			{
				_stopwatch.Start();
				Navigation.PushAsync(new ClipImagesPerformanceTestPage(_stopwatch));
			};
		}
	}

	public class ImagesPerformanceTestPage : ContentPage
	{
		readonly Stopwatch _stopwatch;

		public ImagesPerformanceTestPage(Stopwatch stopwatch)
		{
			_stopwatch = stopwatch;

			Title = "Images";

			var scrollView = new ScrollView();

			var imagesGrid = new Grid();

			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());
			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());
			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());
			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());

			for (int i = 0; i < 25; i++)
			{
				imagesGrid.RowDefinitions.Add(new RowDefinition { Height = 50 });

				for (int j = 0; j < 4; j++)
				{
					var image = new Image
					{
						Aspect = Aspect.AspectFill,
						Source = "crimson.jpg",
					};
					Grid.SetRow(image, i);
					Grid.SetColumn(image, j);
					imagesGrid.Children.Add(image);
				}
			}

			scrollView.Content = imagesGrid;

			Content = scrollView;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_stopwatch.Stop();
			TimeSpan elapsed = _stopwatch.Elapsed;
			string message = $"{elapsed.TotalMilliseconds} ms";
			Debug.WriteLine(message);
			_stopwatch.Reset();
		}
	}

	public class ClipImagesPerformanceTestPage : ContentPage
	{
		readonly Stopwatch _stopwatch;

		public ClipImagesPerformanceTestPage(Stopwatch stopwatch)
		{
			_stopwatch = stopwatch;

			Title = "Clip Images";

			var scrollView = new ScrollView();

			var imagesGrid = new Grid();

			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());
			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());
			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());
			imagesGrid.ColumnDefinitions.Add(new ColumnDefinition());

			for (int i = 0; i < 25; i++)
			{
				imagesGrid.RowDefinitions.Add(new RowDefinition { Height = 50 });

				for (int j = 0; j < 4; j++)
				{
					var image = new Image
					{
						Aspect = Aspect.AspectFill,
						Source = "crimson.jpg",
						Clip = new EllipseGeometry
						{
							Center = new Point(25, 25),
							RadiusX = 25,
							RadiusY = 25
						}
					};
					Grid.SetRow(image, i);
					Grid.SetColumn(image, j);
					imagesGrid.Children.Add(image);
				}
			}

			scrollView.Content = imagesGrid;

			Content = scrollView;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_stopwatch.Stop();
			TimeSpan elapsed = _stopwatch.Elapsed;
			string message = $"{elapsed.TotalMilliseconds} ms";
			Debug.WriteLine(message);
			_stopwatch.Reset();
		}
	}
}