using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
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
					HeightRequest = 100,
				};

				var image = new Image
				{
					WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(2, 5, 2, 2),
					AutomationId = "photo"
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(2, 0, 2, 2),
					BackgroundColor = Colors.Blue
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
					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition { Height = GridLength.Auto } },
					WidthRequest = 280,
					HeightRequest = 310
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
					BackgroundColor = Colors.Aquamarine,
					HorizontalTextAlignment = TextAlignment.Center
				};

				caption.SetBinding(Label.TextProperty, new Binding("Caption"));

				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate CarouselXamlTemplate()
		{
			return new DataTemplate(() =>
			{
				return new CarouselViewGalleries.ExampleTemplateCarousel();
			});
		}

		public static DataTemplate CarouselTemplate()
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid
				{
					BackgroundColor = Colors.LightBlue,
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

				var caption = new Button
				{
					BackgroundColor = Colors.Gray,
					HorizontalOptions = LayoutOptions.Fill,
					Margin = new Thickness(5)
				};

				caption.SetBinding(Button.TextProperty, new Binding("Caption"));
				caption.SetBinding(Button.AutomationIdProperty, new Binding("Caption"));
				caption.Clicked += (sender, e) =>
				{
					//App.Current.MainPage.DisplayAlertAsync("Button works", (sender as Button).Text, "Ok");
				};

				grid.Children.Add(image);
				grid.Children.Add(caption);

				Grid.SetRow(caption, 1);

				var border = new Border
				{
					Padding = new Thickness(5),
					BackgroundColor = Colors.Transparent,
					Content = grid
				};

				return border;
			});
		}

		public static DataTemplate IndicatorTemplate()
		{
			return new DataTemplate(() =>
			{
				var image = new Image
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Aspect = Aspect.AspectFill,
					Source = new FontImageSource
					{
						FontFamily = DefaultFontFamily(),
						Glyph = "\uf30c",
					},
					HeightRequest = 10,
					WidthRequest = 10
				};

				return image;
			});
		}

		public static DataTemplate ScrollToIndexTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					BackgroundColor = Colors.Bisque,

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
					HeightRequest = 40,
					WidthRequest = 100,
					BackgroundColor = Colors.Crimson,
					Text = "Caption"
				};

				caption.SetBinding(Label.TextProperty, new Binding("Index", stringFormat: "Index {0}"));

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
					BackgroundColor = Colors.Bisque,

					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition { Height = GridLength.Auto } },
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
					HeightRequest = 40,
					WidthRequest = 100,
					BackgroundColor = Colors.Crimson,
					Text = "Caption"
				};

				caption.SetBinding(Label.TextProperty, new Binding("Date", stringFormat: "{0:d}"));

				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate PropagationTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					BackgroundColor = Colors.Bisque,
					RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } },
					HeightRequest = 140
				};

				var buttonLayout = new StackLayout { Orientation = StackOrientation.Horizontal };

				var button1 = new Button
				{
					Margin = new Thickness(5),
					Text = "Button 1"
				};

				var button2 = new Button
				{
					Margin = new Thickness(5),
					Text = "Button 2"
				};

				var button3 = new Button
				{
					Margin = new Thickness(5),
					Text = "Button 3"
				};

				buttonLayout.Children.Add(button1);
				buttonLayout.Children.Add(button2);
				buttonLayout.Children.Add(button3);

				templateLayout.Children.Add(buttonLayout);

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
				var layout = new Border();

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

		public static DataTemplate RandomSizeTemplate()
		{
			var indexHeightConverter = new IndexRequestRandomConverter(50, 150);
			var indexWidthConverter = new IndexRequestRandomConverter(50, 150);
			var colorConverter = new IndexColorConverter();

			return new DataTemplate(() =>
			{
				var layout = new Border();

				layout.SetBinding(VisualElement.HeightRequestProperty, new Binding("Index", converter: indexHeightConverter));
				layout.SetBinding(VisualElement.WidthRequestProperty, new Binding("Index", converter: indexWidthConverter));
				layout.SetBinding(VisualElement.BackgroundColorProperty, new Binding("Index", converter: colorConverter));

				var label = new Label { FontSize = 30 };
				label.SetBinding(Label.TextProperty, new Binding("Index"));

				layout.Content = label;

				return layout;
			});
		}

		public static DataTemplate DynamicTextTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					RowDefinitions = new RowDefinitionCollection
					{
						new RowDefinition() { Height = GridLength.Auto },
						new RowDefinition() { Height = GridLength.Auto },
						new RowDefinition() { Height = GridLength.Auto }
					},
					BackgroundColor = Colors.LightGoldenrodYellow,
					Margin = 10
				};

				var frame = new Border
				{
					HeightRequest = 50,
					WidthRequest = 200,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(2, 5, 2, 2),
					AutomationId = "frame",
					BackgroundColor = Colors.CadetBlue
				};

				var date = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(2, 0, 2, 2)
				};

				date.SetBinding(Label.TextProperty, new Binding("Date"));

				frame.Content = date;

				var caption = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(2, 0, 2, 2),
					LineBreakMode = LineBreakMode.WordWrap,
					MaxLines = 10
				};

				caption.SetBinding(Label.TextProperty, new Binding("Caption"));

				var more = new Button { Text = "More Text" };
				var less = new Button { Text = "Less Text" };

				var buttonLayout = new StackLayout
				{
					Children = { more, less },
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.Center
				};

				more.SetBinding(Button.CommandProperty, new Binding("MoreCommand"));
				less.SetBinding(Button.CommandProperty, new Binding("LessCommand"));

				templateLayout.Children.Add(frame);
				templateLayout.Children.Add(caption);
				templateLayout.Children.Add(buttonLayout);

				Grid.SetRow(buttonLayout, 1);
				Grid.SetRow(caption, 2);

				var rootLayout = new StackLayout
				{
					Children = { templateLayout }
				};

				return rootLayout;
			});
		}

		public static DataTemplate SpacingTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new Grid
				{
					RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition() },
					WidthRequest = 120,
					HeightRequest = 100,
					BackgroundColor = Colors.BlanchedAlmond
				};

				var image = new Image
				{
					HeightRequest = 100,
					WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(2, 5, 2, 2),
					AutomationId = "photo"
				};

				image.SetBinding(Image.SourceProperty, new Binding("Image"));

				var caption = new Label
				{
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Fill,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(2, 0, 2, 2)
				};

				caption.SetBinding(Label.TextProperty, new Binding("Caption"));

				templateLayout.Children.Add(image);
				templateLayout.Children.Add(caption);

				Grid.SetRow(caption, 1);

				return templateLayout;
			});
		}

		public static DataTemplate GroupItemTemplate()
		{
			return new DataTemplate(() =>
			{
				var templateLayout = new StackLayout();

				var label = new Label
				{
					Margin = new Thickness(5, 0, 0, 0),
				};
				label.SetBinding(Label.TextProperty, new Binding("Name"));

				templateLayout.Children.Add(label);

				return templateLayout;
			});
		}

		public static DataTemplate GroupHeaderTemplate()
		{
			return new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.LightGreen,
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
				};
				label.SetBinding(Label.TextProperty, new Binding("Name"));

				return label;
			});
		}

		public static DataTemplate GroupFooterTemplate()
		{
			return new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.Orange,
					Margin = new Thickness(0, 0, 0, 15),
				};
				label.SetBinding(Label.TextProperty, new Binding("Count", stringFormat: "Total members: {0:D}"));

				return label;
			});
		}

		static void More_Clicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		static string DefaultFontFamily()
		{
			var fontFamily = "";
			if (DeviceInfo.Platform == DevicePlatform.iOS)
				fontFamily = "Ionicons";
			else if (DeviceInfo.Platform == DevicePlatform.WinUI)
				fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
			else
				fontFamily = "fonts/ionicons.ttf#";
			return fontFamily;
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

			public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
			{
				var index = (int)value!;

				return index < _cutoff ? _lowValue : (object)_highValue;
			}

			public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
		}

		class IndexRequestRandomConverter : IValueConverter
		{
			readonly int _lowValue;
			readonly int _highValue;
			readonly Random _random;
			readonly Dictionary<int, int> _dictionary = new Dictionary<int, int>();

			public IndexRequestRandomConverter(int lowValue, int highValue)
			{
				_lowValue = lowValue;
				_highValue = highValue;
				_random = new Random(DateTime.UtcNow.Millisecond);
			}

			public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
			{
				var index = (int)value!;
				if (!_dictionary.ContainsKey(index))
				{
					_dictionary[index] = _random.Next(_lowValue, _highValue);
				}

				return _dictionary[index];
			}

			public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
		}

		class IndexColorConverter : IValueConverter
		{
			Color[] _colors = new Color[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Orange, Colors.BlanchedAlmond };

			public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
			{
				var index = (int)value!;
				return _colors[index % _colors.Length];
			}

			public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
		}
	}
}
