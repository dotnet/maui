using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.LifecycleEvents;
using Debug = System.Diagnostics.Debug;
using GradientStop = Microsoft.Maui.Controls.GradientStop;

namespace Maui.Controls.Sample.Pages
{
	public class MainPage : BasePage
	{
		readonly IServiceProvider _services;
		readonly MainPageViewModel _viewModel;

		public MainPage(IServiceProvider services, MainPageViewModel viewModel)
		{
			BackgroundColor = Colors.White;
			ToolbarItems.Add(new ToolbarItem()
			{
				Text = "Page"
			});

			Title = "Welcome to the Samples";
			_services = services;
			BindingContext = _viewModel = viewModel;

			SetupMauiLayout();
			NavigationPage.SetHasNavigationBar(this, false);

			//SetupCompatibilityLayout();
			//SetupVisibilityTest();
		}

		public class VisibilityLabel : Label, IFrameworkElement
		{
			private Visibility _visibility;

			public void SetVisibility(Visibility visibility)
			{
				_visibility = visibility;
				Handler?.UpdateValue(nameof(Visibility));
			}

			Visibility IFrameworkElement.Visibility
			{
				get
				{
					return _visibility;
				}
			}
		}

		const string LoremIpsum =
			"Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
			"Quisque ut dolor metus. Duis vel iaculis mauris, sit amet finibus mi. " +
			"Etiam congue ornare risus, in facilisis libero tempor eget. " +
			"Phasellus mattis mollis libero ut semper. In sit amet sapien odio. " +
			"Sed interdum ullamcorper dui eu rutrum. Vestibulum non sagittis justo. " +
			"Cras rutrum scelerisque elit, et porta est lobortis ac. " +
			"Pellentesque eu ornare tortor. Sed bibendum a nisl at laoreet.";

		void SetupMauiLayout()
		{
			var verticalStack = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.AntiqueWhite };
			var horizontalStack = new HorizontalStackLayout() { Spacing = 2, BackgroundColor = Colors.CornflowerBlue };

			//verticalStack.Add(CreateSampleGrid());
			verticalStack.Add(CreateResizingButton());

			AddTextResizeDemo(verticalStack);

			verticalStack.Add(new Label { Text = " ", Padding = new Thickness(10) });
			var label = new Label { Text = "End-aligned text", BackgroundColor = Colors.Fuchsia, HorizontalTextAlignment = TextAlignment.End };
			label.Margin = new Thickness(15, 10, 20, 15);

			SemanticProperties.SetHint(label, "Hint Text");
			SemanticProperties.SetDescription(label, "Description Text");

			verticalStack.Add(label);
			verticalStack.Add(new Label { Text = "This should be BIG text!", FontSize = 24, HorizontalOptions = LayoutOptions.End });

			SemanticProperties.SetHeadingLevel((BindableObject)verticalStack.Children.Last(), SemanticHeadingLevel.Level1);
			verticalStack.Add(new Label { Text = "This should be BOLD text!", FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center });
			verticalStack.Add(new Label { Text = "This should have character spacing!", CharacterSpacing = 3 });
			verticalStack.Add(new Label { Text = "This should be a CUSTOM font!", FontFamily = "Dokdo" });
			verticalStack.Add(
				new Button
				{
					Text = "Push a Page",
					Command = new Command(async () =>
					{
						await Navigation.PushAsync(new SemanticsPage());
					})
				}
			);

			verticalStack.Add(new Label { Text = "This should have padding", Padding = new Thickness(40), BackgroundColor = Colors.LightBlue });
			verticalStack.Add(new Label { Text = LoremIpsum });
			verticalStack.Add(new Label { Text = LoremIpsum, MaxLines = 2 });
			verticalStack.Add(new Label { Text = LoremIpsum, LineBreakMode = LineBreakMode.TailTruncation });
			verticalStack.Add(new Label { Text = LoremIpsum, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation });
			verticalStack.Add(new Label { Text = "This should have five times the line height! " + LoremIpsum, LineHeight = 5, MaxLines = 2 });
			verticalStack.Add(new Label
			{
				FontSize = 24,
				Text = "LinearGradient Text",
				Background = new LinearGradientBrush(
				new GradientStopCollection
				{
 					new GradientStop(Colors.Green, 0),
 					new GradientStop(Colors.Blue, 1)
				},
				new Point(0, 0),
				new Point(1, 0))
			});
			verticalStack.Add(new Label
			{
				Text = "RadialGradient",
				Padding = new Thickness(30),
				Background = new RadialGradientBrush(
 				new GradientStopCollection
 				{
 					new GradientStop(Colors.DarkBlue, 0),
 					new GradientStop(Colors.Yellow, 0.6f),
 					new GradientStop(Colors.LightPink, 1)
 				},
 				new Point(0.5, 0.5),
 				0.3f)
			});

			SemanticProperties.SetHeadingLevel((BindableObject)verticalStack.Children.Last(), SemanticHeadingLevel.Level2);

			var visibleClearButtonEntry = new Entry() { ClearButtonVisibility = ClearButtonVisibility.WhileEditing, Placeholder = "This Entry will show clear button if has input." };
			var hiddenClearButtonEntry = new Entry() { ClearButtonVisibility = ClearButtonVisibility.Never, Placeholder = "This Entry will not..." };

			verticalStack.Add(visibleClearButtonEntry);
			verticalStack.Add(hiddenClearButtonEntry);

			verticalStack.Add(new Editor { Placeholder = "This is an editor placeholder." });
			verticalStack.Add(new Editor { Placeholder = "Green Text Color.", TextColor = Colors.Green });
			var paddingButton = new Button
			{
				Padding = new Thickness(40),
				Text = "This button has a padding!!",
				BackgroundColor = Colors.Purple,
			};

			verticalStack.Add(paddingButton);

			var underlineLabel = new Label { Text = "underline", TextDecorations = TextDecorations.Underline };
			verticalStack.Add(underlineLabel);

			verticalStack.Add(new ActivityIndicator());
			verticalStack.Add(new ActivityIndicator { Color = Colors.Red, IsRunning = true });

			var button = new Button() { Text = _viewModel.Text, WidthRequest = 200 };
			button.Clicked += async (sender, e) =>
			{
				var events = _services.GetRequiredService<ILifecycleEventService>();
				events.InvokeEvents<Action<string>>("CustomEventName", action => action("VALUE"));

				var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Lowest));
				Debug.WriteLine($"I tracked you down to {location.Latitude}, {location.Longitude}! You can't hide!");
			};

			var button2 = new Button()
			{
				TextColor = Colors.Green,
				Text = "Hello I'm a button",
				//	BackgroundColor = Color.Purple,
				Margin = new Thickness(12)
			};

			horizontalStack.Add(button);
			horizontalStack.Add(button2);

			horizontalStack.Add(new Label { Text = "And these buttons are in a HorizontalStackLayout", VerticalOptions = LayoutOptions.Center });

			verticalStack.Add(horizontalStack);

			verticalStack.Add(new Button { Text = "CharacterSpacing" });
			verticalStack.Add(new Button { CharacterSpacing = 8, Text = "CharacterSpacing" });

			verticalStack.Add(new RedButton { Text = "Dynamically Registered" });
			verticalStack.Add(new CustomButton { Text = "Button Registered to Compat Renderer" });
			
			var checkbox = new CheckBox();
			checkbox.CheckedChanged += (sender, e) =>
			{
				Debug.WriteLine($"Checked Changed to '{e.Value}'");
			};
			verticalStack.Add(checkbox);
			verticalStack.Add(new CheckBox { BackgroundColor = Colors.LightPink });
			verticalStack.Add(new CheckBox { IsChecked = true, Color = Colors.Aquamarine });

			var editor = new Editor();
			editor.Completed += (sender, args) =>
			{
				Debug.WriteLine($"Editor Completed");
			};

			verticalStack.Add(editor);
			verticalStack.Add(new Editor { Text = "Editor" });
			verticalStack.Add(new Editor { Text = "Lorem ipsum dolor sit amet", MaxLength = 10 });
			verticalStack.Add(new Editor { Text = "Predictive Text Off", IsTextPredictionEnabled = false });
			verticalStack.Add(new Editor { Text = "Lorem ipsum dolor sit amet", FontSize = 10, FontFamily = "Dokdo" });
			verticalStack.Add(new Editor { Text = "ReadOnly Editor", IsReadOnly = true });


			var entry = new Entry();
			entry.TextChanged += (sender, e) =>
			{
				Debug.WriteLine($"Text Changed from '{e.OldTextValue}' to '{e.NewTextValue}'");
			};

			var entryMargin = new Thickness(10, 0);

			verticalStack.Add(entry);
			verticalStack.Add(new Entry { Text = "Entry", TextColor = Colors.DarkRed, FontFamily = "Dokdo", MaxLength = -1, Margin = entryMargin });
			verticalStack.Add(new Entry { IsPassword = true, TextColor = Colors.Black, Placeholder = "Pasword Entry", Margin = entryMargin });
			verticalStack.Add(new Entry { IsTextPredictionEnabled = false });
			verticalStack.Add(new Entry { Placeholder = "This should be placeholder text", Margin = entryMargin });
			verticalStack.Add(new Entry { Text = "This should be read only property", IsReadOnly = true, Margin = entryMargin });
			verticalStack.Add(new Entry { MaxLength = 5, Placeholder = "MaxLength text", Margin = entryMargin });
			verticalStack.Add(new Entry { Text = "This should be text with character spacing", CharacterSpacing = 10 });
			verticalStack.Add(new Entry { Keyboard = Keyboard.Numeric, Placeholder = "Numeric Entry" });
			verticalStack.Add(new Entry { Keyboard = Keyboard.Email, Placeholder = "Email Entry" });
			verticalStack.Add(new Entry { Placeholder = "This is a blue text box", BackgroundColor = Colors.CornflowerBlue });

			verticalStack.Add(new ProgressBar { Progress = 0.5 });
			verticalStack.Add(new ProgressBar { Progress = 0.5, BackgroundColor = Colors.LightCoral });
			verticalStack.Add(new ProgressBar { Progress = 0.5, ProgressColor = Colors.Purple });

			var searchBar = new SearchBar
			{
				CharacterSpacing = 4,
				Text = "A search query"
			};
			verticalStack.Add(searchBar);

			var placeholderSearchBar = new SearchBar
			{
				Placeholder = "Placeholder"
			};
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

			var picker = new Picker { Title = "Select a monkey", FontFamily = "Dokdo", HorizontalTextAlignment = TextAlignment.Center };

			picker.ItemsSource = monkeyList;
			verticalStack.Add(picker);

			verticalStack.Add(new Slider());

			verticalStack.Add(new Stepper());
			verticalStack.Add(new Stepper { BackgroundColor = Colors.IndianRed });
			verticalStack.Add(new Stepper { Minimum = 0, Maximum = 10, Value = 5 });

			verticalStack.Add(new Switch());
			verticalStack.Add(new Switch() { OnColor = Colors.Green });
			verticalStack.Add(new Switch() { ThumbColor = Colors.Yellow });
			verticalStack.Add(new Switch() { OnColor = Colors.Green, ThumbColor = Colors.Yellow });

			verticalStack.Add(new DatePicker());
			verticalStack.Add(new DatePicker { CharacterSpacing = 6 });
			verticalStack.Add(new DatePicker { FontSize = 24 });

			verticalStack.Add(new TimePicker());
			verticalStack.Add(new TimePicker { Time = TimeSpan.FromHours(8), CharacterSpacing = 6 });

			verticalStack.Add(new Label { Text = "IMAGES (static | animated):" });
			verticalStack.Add(CreateImagesGrid());

			Content = new ScrollView
			{
				Content = verticalStack
			};
		}

		Button CreateResizingButton()
		{
			var initialWidth = 200;
			var otherWidth = 100;

			var initialHeight = 80;
			var otherHeight = 140;

			var count = 1;

			var resizeButton = new Button
			{
				Text = "Resize",
				BackgroundColor = Colors.Gray,
				WidthRequest = initialWidth,
				HeightRequest = initialHeight
			};

			resizeButton.Clicked += (sender, args) =>
			{

				count += 1;

				if (count == 1)
				{
					resizeButton.WidthRequest = initialWidth;
					resizeButton.HeightRequest = initialHeight;
				}
				else if (count == 2)
				{
					resizeButton.WidthRequest = otherWidth;
					resizeButton.HeightRequest = otherHeight;
				}
				else
				{
					// Go back to using whatever the layout gives us
					resizeButton.WidthRequest = -1;
					resizeButton.HeightRequest = -1;
					count = 0;
				}
			};

			return resizeButton;
		}

		void SetupCompatibilityLayout()
		{
			var verticalStack = new StackLayout() { Spacing = 5, BackgroundColor = Colors.AntiqueWhite };
			var horizontalStack = new StackLayout() { Orientation = StackOrientation.Horizontal, Spacing = 2, BackgroundColor = Colors.CornflowerBlue };

			var label = new Label { Text = "This will disappear in ~5 seconds", BackgroundColor = Colors.Fuchsia };
			label.Margin = new Thickness(15, 10, 20, 15);

			verticalStack.Add(label);

			var button = new Button() { Text = _viewModel.Text, WidthRequest = 200 };
			var button2 = new Button()
			{
				TextColor = Colors.Green,
				Text = "Hello I'm a button",
				BackgroundColor = Colors.Purple,
				Margin = new Thickness(12)
			};

			horizontalStack.Add(button);
			horizontalStack.Add(button2);
			horizontalStack.Add(new Label { Text = "And these buttons are in a HorizontalStackLayout" });

			verticalStack.Add(horizontalStack);
			verticalStack.Add(new Slider());
			verticalStack.Add(new Switch());
			verticalStack.Add(new Switch() { OnColor = Colors.Green });
			verticalStack.Add(new Switch() { ThumbColor = Colors.Yellow });
			verticalStack.Add(new Switch() { OnColor = Colors.Green, ThumbColor = Colors.Yellow });
			verticalStack.Add(new DatePicker());
			verticalStack.Add(new TimePicker());
			verticalStack.Add(new Image()
			{
				Source = "dotnet_bot.png"
			});

			Content = verticalStack;
		}

		IView CreateImagesGrid()
		{
			var layout = new Microsoft.Maui.Controls.Layout2.GridLayout { ColumnSpacing = 10, RowSpacing = 10, Margin = 10 };

			layout.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
			layout.AddRowDefinition(new RowDefinition { Height = new GridLength(120) });
			layout.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
			layout.AddRowDefinition(new RowDefinition { Height = new GridLength(120) });
			layout.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
			layout.AddRowDefinition(new RowDefinition { Height = new GridLength(120) });
			layout.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
			layout.AddRowDefinition(new RowDefinition { Height = new GridLength(120) });
			layout.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
			layout.AddRowDefinition(new RowDefinition { Height = new GridLength(120) });

			layout.AddColumnDefinition(new ColumnDefinition { Width = new GridLength(120) });
			layout.AddColumnDefinition(new ColumnDefinition { Width = new GridLength(120) });

			var row = -1;

			Add(new Label { Text = "App Bundle", WidthRequest = 150 }, row: (row += 2) - 1, col: 0, colSpan: 2);
			Add(new Image { Source = "dotnet_bot.png" }, row: row, col: 0);
			Add(new Image { Source = "animated_heart.gif", IsAnimationPlaying = true }, row: row, col: 1);

			Add(new Label { Text = "File", WidthRequest = 150 }, row: (row += 2) - 1, col: 0, colSpan: 2);
			Add(new Image { Source = CopyLocal("dotnet_bot.png") }, row: row, col: 0);
			Add(new Image { Source = CopyLocal("animated_heart.gif"), IsAnimationPlaying = true }, row: row, col: 1);

			Add(new Label { Text = "Font", WidthRequest = 150 }, row: (row += 2) - 1, col: 0, colSpan: 2);
			Add(new Image { Source = new FontImageSource { FontFamily = "Ionicons", Glyph = "\uf2fe" }, BackgroundColor = Color.FromUint(0xFF512BD4), Aspect = Aspect.Center }, row: row, col: 0);
			Add(new Image { Source = new FontImageSource { FontFamily = "Dokdo", Glyph = "M" }, BackgroundColor = Color.FromUint(0xFF512BD4), Aspect = Aspect.Center }, row: row, col: 1);

			Add(new Label { Text = "URI", WidthRequest = 150 }, row: (row += 2) - 1, col: 0, colSpan: 2);
			Add(new Image { Source = "https://raw.githubusercontent.com/dotnet-foundation/swag/05cc70d33fa8c310147b9bd70ae9e103a072cae0/dotnet-bot/dotnet-bot-pot.png" }, row: row, col: 0);
			Add(new Image { Source = "https://raw.githubusercontent.com/mono/SkiaSharp/6753bfad91dce1894c69084555dab6494efa90eb/samples/Gallery/Shared/Media/animated-heart.gif", IsAnimationPlaying = true }, row: row, col: 1);

			Add(new Label { Text = "Stream", WidthRequest = 150 }, row: (row += 2) - 1, col: 0, colSpan: 2);
			Add(new Image { Source = ImageSource.FromStream(() => GetEmbedded("dotnet_bot.png")) }, row: row, col: 0);
			Add(new Image { Source = ImageSource.FromStream(() => GetEmbedded("animated_heart.gif")), IsAnimationPlaying = true }, row: row, col: 1);

			return layout;

			void Add(IView view, int row = 0, int col = 0, int rowSpan = 1, int colSpan = 1)
			{
				layout.Add(view);
				layout.SetRow(view, row);
				layout.SetRowSpan(view, rowSpan);
				layout.SetColumn(view, col);
				layout.SetColumnSpan(view, colSpan);
			}

			string CopyLocal(string embeddedPath)
			{
				var path = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString("N"));

				using var stream = GetEmbedded(embeddedPath);
				using var file = File.Create(path);
				stream.CopyTo(file);

				return path;
			}

			Stream GetEmbedded(string embeddedPath)
			{
				var assembly = GetType().Assembly;
				var name = assembly
					.GetManifestResourceNames()
					.First(n => n.EndsWith(embeddedPath, StringComparison.InvariantCultureIgnoreCase));
				return assembly.GetManifestResourceStream(name);
			}
		}

		IView CreateSampleGrid()
		{
			var layout = new Microsoft.Maui.Controls.Layout2.GridLayout() { ColumnSpacing = 0, RowSpacing = 0 };

			layout.AddRowDefinition(new RowDefinition() { Height = new GridLength(40) });
			layout.AddRowDefinition(new RowDefinition() { Height = GridLength.Auto });

			layout.AddColumnDefinition(new ColumnDefinition() { Width = new GridLength(100) });
			layout.AddColumnDefinition(new ColumnDefinition() { Width = new GridLength(100) });

			var topLeft = new Label { Text = "Top Left", BackgroundColor = Colors.LightBlue };
			layout.Add(topLeft);

			var bottomLeft = new Label { Text = "Bottom Left", BackgroundColor = Colors.Lavender };
			layout.Add(bottomLeft);
			layout.SetRow(bottomLeft, 1);

			var topRight = new Label { Text = "Top Right", BackgroundColor = Colors.Orange };
			layout.Add(topRight);
			layout.SetColumn(topRight, 1);

			var bottomRight = new Label { Text = "Bottom Right", BackgroundColor = Colors.MediumPurple };
			layout.Add(bottomRight);
			layout.SetRow(bottomRight, 1);
			layout.SetColumn(bottomRight, 1);

			layout.BackgroundColor = Colors.Chartreuse;

			return layout;
		}

		void AddTextResizeDemo(Microsoft.Maui.ILayout layout)
		{
			var resizeTestButton = new Button { Text = "Resize Test" };

			var resizeTestLabel = new Label { Text = "Short Text", BackgroundColor = Colors.LightBlue, HorizontalOptions = LayoutOptions.Start };
			var explicitWidthTestLabel = new Label { Text = "Short Text", BackgroundColor = Colors.LightGreen, WidthRequest = 200 };
			var widthAndHeightTestLabel = new Label { Text = "Short Text", BackgroundColor = Colors.MediumSeaGreen, WidthRequest = 150, HeightRequest = 40 };

			resizeTestButton.Clicked += (sender, args) =>
			{
				if (resizeTestLabel.Text == "Short Text")
				{
					resizeTestLabel.Text = LoremIpsum;
					explicitWidthTestLabel.Text = LoremIpsum;
					widthAndHeightTestLabel.Text = LoremIpsum;
				}
				else
				{
					resizeTestLabel.Text = "Short Text";
					explicitWidthTestLabel.Text = "Short Text";
					widthAndHeightTestLabel.Text = "Short Text";
				}
			};

			layout.Add(resizeTestButton);
			layout.Add(resizeTestLabel);
			layout.Add(widthAndHeightTestLabel);
			layout.Add(explicitWidthTestLabel);
		}

		void SetupVisibilityTest()
		{
			var layout = new VerticalStackLayout() { BackgroundColor = Colors.BurlyWood };

			var button1 = new Button { Text = "Controls", Margin = new Thickness(0, 40) };

			var button2 = new Button { Text = "MAUI" };

			var controlsLabel = new Label { Text = "Controls Label" };
			controlsLabel.IsVisible = true;

			var alwaysVisible = new Label { Text = "Always visible" };

			var mauiLabel = new VisibilityLabel() { Text = "Core Label" };

			button1.Clicked += (sender, args) =>
			{
				controlsLabel.IsVisible = !controlsLabel.IsVisible;
			};

			button2.Clicked += (sender, args) =>
			{
				switch ((mauiLabel as IFrameworkElement).Visibility)
				{
					case Visibility.Visible:
						mauiLabel.SetVisibility(Visibility.Hidden);
						break;
					case Visibility.Hidden:
						mauiLabel.SetVisibility(Visibility.Collapsed);
						break;
					case Visibility.Collapsed:
						mauiLabel.SetVisibility(Visibility.Visible);
						break;
				}
			};

			layout.Add(button1);
			layout.Add(button2);
			layout.Add(controlsLabel);
			layout.Add(mauiLabel);
			layout.Add(alwaysVisible);

			Content = layout;
		}
	}
}
