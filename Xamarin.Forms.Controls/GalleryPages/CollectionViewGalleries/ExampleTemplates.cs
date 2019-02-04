using System;
using System.Globalization;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ExampleTemplates
	{
		public static DataTemplate PhotoTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition() },
					WidthRequest = 200,
					HeightRequest = 100
				};

				var image = new Image
				{
					HeightRequest = 100, WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center
				};

				caption.SetBinding(Label.TextProperty, new Binding("Caption"));
				
				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate SnapPointsTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition {Height = GridLength.Auto} },
					WidthRequest = 280,
					HeightRequest = 310,
				};

				var image = new Image
				{
					Margin = new Thickness(5),
					HeightRequest = 280,
					WidthRequest = 280,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Aspect = Aspect.AspectFit
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					BackgroundColor = Color.Aquamarine,
					HorizontalTextAlignment = TextAlignment.Center
				};

				caption.SetBinding(Label.TextProperty, new Binding("Caption"));
				
				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate CarouselTemplate()
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid
				{
					BackgroundColor = Color.LightBlue,
					RowDefinitions = new RowDefinitionCollection
					{
						new RowDefinition(),
						new RowDefinition { Height = GridLength.Auto }
					}
				};

				var image = new Image
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Aspect = Aspect.AspectFill
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					BackgroundColor = Color.Gray,
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(5)
				};

				caption.SetBinding(Label.TextProperty, new Binding("Caption"));
				
				grid.Children.Add(image);
				grid.Children.Add(caption);

				Grid.SetRow(caption, 1);

				var frame = new Frame
				{
					Padding = new Thickness(5),
					Content = grid
				};

				return  frame;
			});
		}

		public static DataTemplate ScrollToIndexTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					BackgroundColor = Color.Bisque,

					RowDefinitions = new RowDefinitionCollection
						{ new RowDefinition(), new RowDefinition { Height = GridLength.Auto } },
					WidthRequest = 100,
					HeightRequest = 140
				};

				var image = new Image
				{
					Margin = new Thickness(5),
					HeightRequest = 100,
					WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Aspect = Aspect.AspectFit
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					HeightRequest = 40, WidthRequest = 100,
					BackgroundColor = Color.Crimson,
					Text = "Caption"
				};

				caption.SetBinding(Label.TextProperty, new Binding("Index", stringFormat:"Index {0}"));

				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate ScrollToItemTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					BackgroundColor = Color.Bisque,

					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition {Height = GridLength.Auto} },
					WidthRequest = 100,
					HeightRequest = 140
				};

				var image = new Image
				{
					Margin = new Thickness(5),
					HeightRequest = 100,
					WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Aspect = Aspect.AspectFit
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					HeightRequest = 40, WidthRequest = 100,
					BackgroundColor = Color.Crimson,
					Text = "Caption"
				};

				caption.SetBinding(Label.TextProperty, new Binding("Date", stringFormat: "{0:d}"));
				
				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate VariableSizeTemplate()
		{
			var indexHeightConverter = new IndexRequestConverter(3, 50, 150);
			var indexWidthConverter = new IndexRequestConverter(3, 100, 300);
			var colorConverter = new IndexColorConverter();

			return new DataTemplate(() =>
			{
				var layout = new Frame();

				layout.SetBinding(VisualElement.HeightRequestProperty, new Binding("Index", converter: indexHeightConverter));
				layout.SetBinding(VisualElement.WidthRequestProperty, new Binding("Index", converter: indexWidthConverter));
				layout.SetBinding(VisualElement.BackgroundColorProperty, new Binding("Index", converter: colorConverter));

				var image = new Image
				{
					Aspect = Aspect.AspectFit
				};

				image.SetBinding(VisualElement.HeightRequestProperty, new Binding("Index", converter: indexHeightConverter));
				image.SetBinding(VisualElement.WidthRequestProperty, new Binding("Index", converter: indexWidthConverter));

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				layout.Content = image;

				return layout;
			});
		}

		class IndexRequestConverter : IValueConverter
		{
			readonly int _cutoff;
			readonly int _lowValue;
			readonly int _highValue;

			public IndexRequestConverter(int cutoff, int lowValue, int highValue)
			{
				_cutoff = cutoff;
				_lowValue = lowValue;
				_highValue = highValue;
			}

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				var index = (int)value;

				return index < _cutoff ? _lowValue : (object)_highValue;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
		}

		class IndexColorConverter : IValueConverter
		{
			Color[] _colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.BlanchedAlmond };

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				var index = (int)value;
				return _colors[index % _colors.Length];
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
		}
	}
}