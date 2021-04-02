using System;
using System.Collections.Generic;
using System.Linq;
using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Graphics;
using Debug = System.Diagnostics.Debug;

namespace Maui.Controls.Sample.Pages
{
	public class MainPage : BasePage
	{
		readonly IServiceProvider _services;
		readonly MainPageViewModel _viewModel;

		public MainPage(IServiceProvider services, MainPageViewModel viewModel)
		{
			_services = services;
			BindingContext = _viewModel = viewModel;

			SetupMauiLayout();
			//SetupCompatibilityLayout();
		}

		void SetupMauiLayout()
		{
			const string loremIpsum =
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
				"Quisque ut dolor metus. Duis vel iaculis mauris, sit amet finibus mi. " +
				"Etiam congue ornare risus, in facilisis libero tempor eget. " +
				"Phasellus mattis mollis libero ut semper. In sit amet sapien odio. " +
				"Sed interdum ullamcorper dui eu rutrum. Vestibulum non sagittis justo. " +
				"Cras rutrum scelerisque elit, et porta est lobortis ac. " +
				"Pellentesque eu ornare tortor. Sed bibendum a nisl at laoreet.";

			var verticalStack = new VerticalStackLayout() { Spacing = 5, BackgroundColor = Colors.AntiqueWhite };
			var horizontalStack = new HorizontalStackLayout() { Spacing = 2, BackgroundColor = Colors.CornflowerBlue };


			verticalStack.Add(CreateSampleGrid());

			verticalStack.Add(new Label { Text = " ", Padding = new Thickness(10) });
			var label = new Label { Text = "End-aligned text", BackgroundColor = Colors.Fuchsia, HorizontalTextAlignment = TextAlignment.End };
			label.Margin = new Thickness(15, 10, 20, 15);

			SemanticProperties.SetHint(label, "Hint Text");
			SemanticProperties.SetDescription(label, "Description Text");

			verticalStack.Add(label);
			verticalStack.Add(new Label { Text = "This should be BIG text!", FontSize = 24, HorizontalOptions = LayoutOptions.End });

			SemanticProperties.SetHeadingLevel((BindableObject)verticalStack.Children.Last(), SemanticHeadingLevel.Level1);
			verticalStack.Add(new Label { Text = "This should be BOLD text!", FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center });
			verticalStack.Add(new Label { Text = "This should be a CUSTOM font!", FontFamily = "Dokdo" });
			verticalStack.Add(new Label { Text = "This should have padding", Padding = new Thickness(40), BackgroundColor = Colors.LightBlue });
			verticalStack.Add(new Label { Text = loremIpsum });
			verticalStack.Add(new Label { Text = loremIpsum, MaxLines = 2 });
			verticalStack.Add(new Label { Text = loremIpsum, LineBreakMode = LineBreakMode.TailTruncation });
			verticalStack.Add(new Label { Text = loremIpsum, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation });
			verticalStack.Add(new Label { Text = "This should have five times the line height! " + loremIpsum, LineHeight = 5, MaxLines = 2 });

			SemanticProperties.SetHeadingLevel((BindableObject)verticalStack.Children.Last(), SemanticHeadingLevel.Level2);

			var visibleClearButtonEntry = new Entry() { ClearButtonVisibility = ClearButtonVisibility.WhileEditing, Placeholder = "This Entry will show clear button if has input." };
			var hiddenClearButtonEntry = new Entry() { ClearButtonVisibility = ClearButtonVisibility.Never, Placeholder = "This Entry will not..." };

			verticalStack.Add(visibleClearButtonEntry);
			verticalStack.Add(hiddenClearButtonEntry);

			verticalStack.Add(new Editor { Placeholder = "This is an editor placeholder." });
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
				BackgroundColor = Colors.Purple,
				Margin = new Thickness(12)
			};

			horizontalStack.Add(button);
			horizontalStack.Add(button2);

			horizontalStack.Add(new Label { Text = "And these buttons are in a HorizontalStackLayout", VerticalOptions = LayoutOptions.Center });

			verticalStack.Add(horizontalStack);

			var paddingButton = new Button
			{
				Padding = new Thickness(40),
				Text = "This button has a padding!!",
				BackgroundColor = Colors.Purple,
			};

			verticalStack.Add(paddingButton);
			verticalStack.Add(new Button { Text = "CharacterSpacing" });
			verticalStack.Add(new Button { CharacterSpacing = 8, Text = "CharacterSpacing" });

			var checkbox = new CheckBox();
			checkbox.CheckedChanged += (sender, e) =>
			{
				Debug.WriteLine($"Checked Changed to '{e.Value}'");
			};
			verticalStack.Add(checkbox);
			verticalStack.Add(new CheckBox { BackgroundColor = Colors.LightPink });
			verticalStack.Add(new CheckBox { IsChecked = true, Color = Colors.Aquamarine });

			verticalStack.Add(new Editor());
			verticalStack.Add(new Editor { Text = "Editor" });
			verticalStack.Add(new Editor { Text = "Lorem ipsum dolor sit amet", MaxLength = 10 });
			verticalStack.Add(new Editor { Text = "Predictive Text Off", IsTextPredictionEnabled = false });
			verticalStack.Add(new Editor { Text = "Lorem ipsum dolor sit amet", FontSize = 10, FontFamily = "dokdo_regular" });
			verticalStack.Add(new Editor { Text = "ReadOnly Editor", IsReadOnly = true });


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
			verticalStack.Add(new Entry { Text = "This should be text with character spacing", CharacterSpacing = 10 });
			verticalStack.Add(new Entry { Keyboard = Keyboard.Numeric, Placeholder = "Numeric Entry" });
			verticalStack.Add(new Entry { Keyboard = Keyboard.Email, Placeholder = "Email Entry" });

			verticalStack.Add(new ProgressBar { Progress = 0.5 });
			verticalStack.Add(new ProgressBar { Progress = 0.5, BackgroundColor = Colors.LightCoral });
			verticalStack.Add(new ProgressBar { Progress = 0.5, ProgressColor = Colors.Purple });

			var searchBar = new SearchBar();
			searchBar.CharacterSpacing = 4;
			searchBar.Text = "A search query";
			verticalStack.Add(searchBar);

			var placeholderSearchBar = new SearchBar();
			placeholderSearchBar.Placeholder = "Placeholder";
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

			var picker = new Picker { Title = "Select a monkey", FontFamily = "Dokdo" };

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

			verticalStack.Add(new Image() { Source = "dotnet_bot.png" });

			Content = new ScrollView
			{
				Content = verticalStack
			};
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
				Source =
				new UriImageSource()
				{
					Uri = new System.Uri("dotnet_bot.png")
				}
			});

			Content = verticalStack;
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }

		IView CreateSampleGrid()
		{
			var layout = new Microsoft.Maui.Controls.Layout2.GridLayout() { ColumnSpacing = 5, RowSpacing = 8 };

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
	}
}