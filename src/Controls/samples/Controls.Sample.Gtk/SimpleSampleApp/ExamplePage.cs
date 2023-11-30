using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Debug = System.Diagnostics.Debug;
using IImage = Microsoft.Maui.Graphics.IImage;
using LineBreakMode = Microsoft.Maui.LineBreakMode;

namespace Maui.SimpleSampleApp
{
	public class ExamplePage : BasePage
	{
		readonly IServiceProvider _services;
		readonly MainPageViewModel _viewModel;

		const string LoremIpsum =
			"Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
			"Quisque ut dolor metus. Duis vel iaculis mauris, sit amet finibus mi. " +
			"Etiam congue ornare risus, in facilisis libero tempor eget. " +
			"Phasellus mattis mollis libero ut semper. In sit amet sapien odio. " +
			"Sed interdum ullamcorper dui eu rutrum. Vestibulum non sagittis justo. " +
			"Cras rutrum scelerisque elit, et porta est lobortis ac. " +
			"Pellentesque eu ornare tortor. Sed bibendum a nisl at laoreet.";

		public ExamplePage(IServiceProvider services, MainPageViewModel viewModel)
		{
			_services = services;
			BindingContext = _viewModel = viewModel;

			// SetupMauiLayoutLayouts();

			// SetupMauiLayoutSimple();
			SetupMauiLayout();
			// SetupMauiLayoutDrawables();
		}

		void SetupMauiLayoutLayouts()
		{
			void Fill(Layout l, string m, int count, Color bkCol)
			{
				var i = 0;

				while (i++ < count)
				{
					var label = new Label
					{
						Text = $"{m} {i}",
						HorizontalTextAlignment = TextAlignment.Center,
						BackgroundColor = bkCol,
						Margin = new Thickness(i),
						LineBreakMode = LineBreakMode.TailTruncation,
						MaxLines = i
					};

					l.Add(label);
				}
			}

			var verticalStack1 = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.WhiteSmoke, };

			var verticalStack2 = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.LightYellow, };

			Fill(verticalStack2, nameof(verticalStack2), 4, Colors.Coral);

			var horizontalStack1 = new HorizontalStackLayout() { Spacing = 5, BackgroundColor = Colors.NavajoWhite, };

			Fill(horizontalStack1, nameof(horizontalStack1), 4, Colors.Aquamarine);

			verticalStack1.Add(verticalStack2);
			verticalStack1.Add(horizontalStack1);

			var verticalStack3 = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.Lime, };

			verticalStack3.Add(new Label
			{
				HorizontalTextAlignment = TextAlignment.Center,
				BackgroundColor = Colors.Bisque,
				Margin = new Thickness(2),
				LineBreakMode = LineBreakMode.TailTruncation,
				MaxLines = 2,
				Text = LoremIpsum
			});

			verticalStack1.Add(verticalStack3);
			Content = verticalStack1;
		}

		void SetupMauiLayoutDrawables()
		{
			Content = CreateShapes();
		}

		void SetupMauiLayoutButtonSpacing()
		{
			var verticalStack = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.WhiteSmoke, };

			var label = new Label { Text = "a label", HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };

			verticalStack.Add(label);

			var label1 = new Label
			{
				Text = "another label",
				HorizontalTextAlignment = TextAlignment.End,
				TextColor = Colors.Coral,
				Margin = new Thickness(5),
				Padding = new Thickness(2),
				FontAttributes = FontAttributes.Italic,
				TextDecorations = TextDecorations.Underline,
				FontSize = 14
			};

			verticalStack.Add(label1);

			var entry = new Entry { Placeholder = "write something" };
			const string ltext = "changed";
			verticalStack.Add(entry);

			var button = new Button
			{
				Padding = new Thickness(10),
				Text = "Change the label!",
				BackgroundColor = Colors.Red,
				TextColor = Colors.Yellow,
				CharacterSpacing = 2
			};

			verticalStack.Add(button);

			button.Clicked += (s, e) =>
			{
				label.Text = label.Text == ltext ? $"{ltext} again" : ltext;

				if (s is Button sender)
				{
					label.TextColor = sender.BackgroundColor;
					label.BackgroundColor = sender.TextColor;
				}
			};

			button.Clicked += (s, e) =>
			{
				entry.Text = string.IsNullOrEmpty(entry.Text) ? "entry text" : null;
			};

			var button2 = new Button
			{
				Padding = new Thickness(10), Text = "Change the button!", BackgroundColor = Colors.Green, TextColor = Colors.Yellow,
			};

			button2.Clicked += (sender, args) =>
			{
				button.CharacterSpacing = button.CharacterSpacing > 1 ? 1 : 2;
			};

			verticalStack.Add(button2);

			var activityIndicator = new ActivityIndicator { Color = Colors.Chartreuse };

			button.Clicked += (s, e) => activityIndicator.IsRunning = !activityIndicator.IsRunning;
			verticalStack.Add(activityIndicator);

			var editor = new Editor { Placeholder = "write something longer", Margin = new Thickness(5), };

			button.Clicked += (s, e) =>
			{
				editor.Text = string.IsNullOrEmpty(editor.Text) ? "editor text" : null;
			};

			verticalStack.Add(editor);

			Content = verticalStack;
		}

		void SetupMauiLayout()
		{
			var verticalStack = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.AntiqueWhite };

			var horizontalStack = new HorizontalStackLayout() { Spacing = 2, BackgroundColor = Colors.CornflowerBlue };

			verticalStack.Add(CreateSampleGrid());

			verticalStack.Add(new Label { Text = " ", Padding = new Thickness(10) });

			var label = new Label { Text = "End-aligned text", BackgroundColor = Colors.Fuchsia, HorizontalTextAlignment = TextAlignment.End, Margin = new Thickness(15, 10, 20, 15) };

			SemanticProperties.SetHint(label, "Hint Text");
			SemanticProperties.SetDescription(label, "Description Text");

			verticalStack.Add(label);

			verticalStack.Add(new Label { Text = "This should be BIG text!", FontSize = 24, HorizontalOptions = LayoutOptions.End });

			SemanticProperties.SetHeadingLevel((BindableObject)verticalStack.Children.Last(), SemanticHeadingLevel.Level1);

			verticalStack.Add(new Label { Text = "This should be BOLD text!", FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center });

			verticalStack.Add(new Label { Text = "This should be a CUSTOM font!", FontFamily = "Dokdo" });

			verticalStack.Add(new Label { Text = "This should have padding", Padding = new Thickness(40), BackgroundColor = Colors.LightBlue });

			verticalStack.Add(new Label { Text = LoremIpsum });

			verticalStack.Add(new Label { Text = LoremIpsum, MaxLines = 2 });

			verticalStack.Add(new Label { Text = LoremIpsum, LineBreakMode = LineBreakMode.TailTruncation });

			verticalStack.Add(new Label { Text = LoremIpsum, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation, WidthRequest = 200 });

			verticalStack.Add(new Label { Text = "This should have five times the line height! " + LoremIpsum, LineHeight = 5, MaxLines = 2 });

			SemanticProperties.SetHeadingLevel((BindableObject)verticalStack.Children.Last(), SemanticHeadingLevel.Level2);

			var visibleClearButtonEntry = new Entry() { ClearButtonVisibility = ClearButtonVisibility.WhileEditing, Placeholder = "This Entry will show clear button if has input." };

			var hiddenClearButtonEntry = new Entry() { ClearButtonVisibility = ClearButtonVisibility.Never, Placeholder = "This Entry will not..." };

			verticalStack.Add(visibleClearButtonEntry);
			verticalStack.Add(hiddenClearButtonEntry);

			var paddingButton = new Button { Padding = new Thickness(40), Text = "This button has a padding!!", BackgroundColor = Colors.Purple, };

			verticalStack.Add(paddingButton);

			var underlineLabel = new Label { Text = (TextDecorations.Underline | TextDecorations.Strikethrough).ToString(), TextDecorations = TextDecorations.Underline | TextDecorations.Strikethrough };

			verticalStack.Add(underlineLabel);
			IImage image = default;

			using (var stream = File.OpenRead("dotnet_bot.png"))
			{
				image = new PlatformImageLoadingService().FromStream(stream);
			}

			var paint = image.AsPaint();

			var labelImage = new Label { Text = "this has backgroudImage", Background = paint };
			// Background is null cause there is no ImageBrush

			if (labelImage.Background != null)
				verticalStack.Add(labelImage);

			var labelG = new Label
			{
				Text = "this has gradient", Background = new RadialGradientBrush(new GradientStopCollection { new(Colors.Aqua, 0), new(Colors.Green, 10), }), Padding = new Thickness(30), Margin = new Thickness(10),
			};

			verticalStack.Add(labelG);

			verticalStack.Add(new ActivityIndicator());

			verticalStack.Add(new ActivityIndicator { Color = Colors.Red, IsRunning = true });

			var button = new Button() { Text = _viewModel.Text, WidthRequest = 200 };

			// button.Clicked += async (sender, e) =>
			// {
			// 	var events = _services.GetRequiredService<ILifecycleEventService>();
			// 	events.InvokeEvents<Action<string>>("CustomEventName", action => action("VALUE"));
			//
			// 	// var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Lowest));
			// 	// Debug.WriteLine($"I tracked you down to {location.Latitude}, {location.Longitude}! You can't hide!");
			// };

			var button2 = new Button()
			{
				TextColor = Colors.Green, Text = "Hello I'm a button", BackgroundColor = Colors.Purple, Margin = new Thickness(12),
			};

			horizontalStack.Add(button);
			horizontalStack.Add(button2);

			horizontalStack.Add(new Label { Text = "And these buttons are in a HorizontalStackLayout", VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.End });

			verticalStack.Add(horizontalStack);

			verticalStack.Add(new Button { CharacterSpacing = 4, Text = "CharacterSpacing 4" });

			var checkbox = new CheckBox();

			checkbox.CheckedChanged += (sender, e) =>
			{
				Debug.WriteLine($"Checked Changed to '{e.Value}'");
			};

			verticalStack.Add(checkbox);
			verticalStack.Add(new CheckBox { BackgroundColor = Colors.LightPink });

			verticalStack.Add(new CheckBox { IsChecked = true, Color = Colors.Aquamarine });

			if (true)
#pragma warning disable 162
			{
				verticalStack.Add(new Editor());
				verticalStack.Add(new Editor { Placeholder = "This is an editor placeholder." });
				verticalStack.Add(new Editor { Text = "Editor" });

				verticalStack.Add(new Editor { Text = "Lorem ipsum dolor sit amet", MaxLength = 10 });

				verticalStack.Add(new Editor { Text = "Predictive Text Off", IsTextPredictionEnabled = false });

				verticalStack.Add(new Editor { Text = "Lorem ipsum dolor sit amet", FontSize = 10, FontFamily = "dokdo_regular" });

				verticalStack.Add(new Editor { Text = "ReadOnly Editor", IsReadOnly = true });
			}
#pragma warning restore 162

			var entry = new Entry();

			entry.TextChanged += (sender, e) =>
			{
				Debug.WriteLine($"Text Changed from '{e.OldTextValue}' to '{e.NewTextValue}'");
			};

			verticalStack.Add(entry);

			verticalStack.Add(new Entry { Text = "Entry", TextColor = Colors.DarkRed, FontFamily = "Dokdo", MaxLength = -1 });

			verticalStack.Add(new Entry { IsPassword = true, TextColor = Colors.Black, Placeholder = "Pasword Entry" });

			verticalStack.Add(new Entry { IsTextPredictionEnabled = false });
			verticalStack.Add(new Entry { Placeholder = "This should be placeholder text" });

			verticalStack.Add(new Entry { Text = "This should be read only property", IsReadOnly = true });

			verticalStack.Add(new Entry { MaxLength = 5, Placeholder = "MaxLength text" });

			var spacingEntry = new Entry { Text = "This should be text with character spacing", CharacterSpacing = 10 };

			verticalStack.Add(spacingEntry);

			button2.Clicked += (s, e) =>
			{
				if (underlineLabel.TextDecorations.HasFlag(TextDecorations.Underline))
				{
					underlineLabel.TextDecorations = TextDecorations.Strikethrough;
					underlineLabel.Text = nameof(TextDecorations.Strikethrough);
					underlineLabel.CharacterSpacing = 2;
				}
				else if (underlineLabel.TextDecorations.HasFlag(TextDecorations.Strikethrough))
				{
					underlineLabel.TextDecorations = TextDecorations.Underline;
					underlineLabel.Text = nameof(TextDecorations.Underline);
					underlineLabel.CharacterSpacing = 1;
				}

				spacingEntry.CharacterSpacing = spacingEntry.CharacterSpacing == 10 ? 5 : 10;
			};

			verticalStack.Add(new Entry { Keyboard = Keyboard.Numeric, Placeholder = "Numeric Entry" });

			verticalStack.Add(new Entry { Keyboard = Keyboard.Email, Placeholder = "Email Entry" });

			verticalStack.Add(new ProgressBar { Progress = 0.5 });

			verticalStack.Add(new ProgressBar { Progress = 0.5, BackgroundColor = Colors.LightCoral });

			verticalStack.Add(new ProgressBar { Progress = 0.5, ProgressColor = Colors.Purple });

			var searchBar = new SearchBar { CharacterSpacing = 4, Text = "A search query" };

			verticalStack.Add(searchBar);

			var placeholderSearchBar = new SearchBar { Placeholder = "Placeholder", BackgroundColor = Colors.Plum };

			verticalStack.Add(placeholderSearchBar);

			var monkeyList = new List<string>
			{
				"Baboon",
				"Capuchin Monkey",
				"Blue Monkey",
				"Squirrel Monkey",
				"Golden Lion Tamarin",
				"Howler Monkey",
				"Japanese Macaque"
			};

			var picker = new Picker
			{
				Title = "Select a monkey",
				FontFamily = "Dokdo",
				TextColor = Colors.Chartreuse,
				BackgroundColor = Colors.Yellow,
				HorizontalTextAlignment = TextAlignment.Center,
				CharacterSpacing = 2,
				ItemsSource = monkeyList,
			};

			verticalStack.Add(picker);

			verticalStack.Add(new Slider { ThumbColor = Colors.Aqua, ThumbImageSource = "rainbow_heart.png" });

			verticalStack.Add(new Stepper());
			verticalStack.Add(new Stepper { BackgroundColor = Colors.IndianRed });

			verticalStack.Add(new Stepper { Minimum = 0, Maximum = 10, Value = 5 });

			verticalStack.Add(new Switch());
			verticalStack.Add(new Switch() { OnColor = Colors.Green });
			verticalStack.Add(new Switch() { ThumbColor = Colors.Yellow });

			verticalStack.Add(new Switch() { OnColor = Colors.Green, ThumbColor = Colors.Yellow });

			verticalStack.Add(new GraphicsView { Drawable = new TextDrawable(), HeightRequest = 50, WidthRequest = 200 });

			verticalStack.Add(new DatePicker());
			verticalStack.Add(new DatePicker { CharacterSpacing = 6 });
			verticalStack.Add(new DatePicker { FontSize = 24 });

			verticalStack.Add(new TimePicker());

			verticalStack.Add(new TimePicker { Time = TimeSpan.FromHours(8), CharacterSpacing = 6 });

			verticalStack.Add(CreateShapes());

			verticalStack.Add(new Image() { Source = "dotnet_bot.png" });

			Content = new ScrollView { Content = verticalStack, Orientation = ScrollOrientation.Both };
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }

		View CreateSampleGrid()
		{
			var layout = new Grid() { ColumnSpacing = 5, RowSpacing = 8 };

			layout.AddRowDefinition(new RowDefinition() { Height = new GridLength(40) });
			layout.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });

			layout.AddColumnDefinition(new ColumnDefinition() { Width = new GridLength(100) });
			layout.AddColumnDefinition(new ColumnDefinition() { Width = new GridLength(100) });

			var topLeft = new Label { Text = "Top Left", BackgroundColor = Colors.LightBlue };

			layout.Add(topLeft);

			var bottomLeft = new Label { Text = "Bottom Left", BackgroundColor = Colors.Lavender, VerticalTextAlignment = TextAlignment.End };

			layout.Add(bottomLeft);
			layout.SetRow(bottomLeft, 1);

			var topRight = new Label
			{
				Text = "Top Right",
				BackgroundColor = Colors.Orange,
				TextColor = Colors.Chocolate,
				VerticalTextAlignment = TextAlignment.Start,
				HorizontalTextAlignment = TextAlignment.End
			};

			layout.Add(topRight);
			layout.SetColumn(topRight, 1);

			var bottomRight = new Label { Text = "Bottom Right", BackgroundColor = Colors.MediumPurple, VerticalTextAlignment = TextAlignment.End, HorizontalTextAlignment = TextAlignment.End };

			layout.Add(bottomRight);
			layout.SetRow(bottomRight, 1);
			layout.SetColumn(bottomRight, 1);

			layout.BackgroundColor = Colors.Chartreuse;

			return layout;
		}

		View CreateShapes()
		{
			var ellipse = new Ellipse
			{
				Fill = new SolidColorBrush(Colors.Red),
				Stroke = new SolidColorBrush(Colors.Blue),
				StrokeThickness = 4,
				HeightRequest = 120,
				WidthRequest = 200
			};

			var line = new Line
			{
				X1 = 0,
				Y1 = 0,
				X2 = 80,
				Y2 = 90,
				Fill = new SolidColorBrush(Colors.Red),
				Stroke = new SolidColorBrush(Colors.Green),
				StrokeThickness = 4,
				StrokeDashArray = new double[] { 2, 2 },
				HeightRequest = 200,
				WidthRequest = 200
			};

			var pathData = (Microsoft.Maui.Controls.Shapes.Geometry)new PathGeometryConverter().ConvertFromInvariantString("M15.999996,0L31.999999,13.000001 15.999996,26.199999 0,13.000001z");

			var path = new Microsoft.Maui.Controls.Shapes.Path
			{
				Data = pathData,
				Fill = new SolidColorBrush(Colors.Pink),
				Stroke = new SolidColorBrush(Colors.Red),
				StrokeThickness = 1,
				HeightRequest = 100,
				WidthRequest = 100
			};

			var polyline = new Polyline
			{
				Points = new[] { new Point(10, 10), new Point(100, 50), new Point(50, 90) },
				Stroke = new SolidColorBrush(Colors.Black),
				StrokeThickness = 2,
				StrokeDashArray = new double[] { 1, 1, 1, 1 },
				HeightRequest = 100,
				WidthRequest = 100
			};

			var polygon = new Polygon
			{
				Points = new[] { new Point(10, 10), new Point(100, 50), new Point(50, 90) },
				Fill = new SolidColorBrush(Colors.LightBlue),
				Stroke = new SolidColorBrush(Colors.Black),
				StrokeThickness = 2,
				StrokeDashArray = new double[] { 2, 2 },
				HeightRequest = 100,
				WidthRequest = 100
			};

			var rectangle = new Microsoft.Maui.Controls.Shapes.Rectangle
			{
				RadiusX = 12,
				RadiusY = 6,
				Fill = new LinearGradientBrush(new Microsoft.Maui.Controls.GradientStopCollection { new(Colors.Green, 0), new(Colors.Blue, 1) }, new Point(0, 0), new Point(1, 0)),
				Stroke = new SolidColorBrush(Colors.Purple),
				StrokeThickness = 8,
				StrokeDashArray = new float[] { 2, 2 },
				HeightRequest = 120,
				WidthRequest = 200
			};

			var verticalStack = new VerticalStackLayout
			{
				ellipse,
				line,
				path,
				polyline,
				polygon,
				rectangle
			};

			return verticalStack;
		}
	}

	class TextDrawable : IDrawable
	{
		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.SaveState();
			canvas.FillColor = Colors.Red;
			canvas.FillRoundedRectangle(0, 0, 200, 50, 10);
			canvas.FontColor = Colors.Yellow;
			canvas.FontSize = 10;
			canvas.DrawString("Drawable", 100, 10, HorizontalAlignment.Center);
			canvas.RestoreState();
		}
	}
}