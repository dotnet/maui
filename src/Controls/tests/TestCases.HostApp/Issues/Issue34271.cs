namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34271, "CollectionView scroll position resets to top after ScrollTo last item when Picker is dismissed", PlatformAffected.macOS)]
public class Issue34271 : ContentPage
{
	// Varying image heights widen the gap between UIKit's estimated contentSize
	// (NSCollectionLayoutDimension.CreateEstimated(30f)) and actual contentSize,
	// making the silent offset reset more pronounced and reliably testable.
	public record Monkey(string Name, string Location, double ImageHeight);

	readonly List<Monkey> _monkeys =
	[
		new("Baboon",                   "Africa & Asia", 40),
		new("Capuchin Monkey",          "Central & South America", 80),
		new("Blue Monkey",              "Central and East Africa", 50),
		new("Squirrel Monkey",          "Central & South America", 100),
		new("Golden Lion Tamarin",      "Brazil", 45),
		new("Howler Monkey",            "South America", 90),
		new("Japanese Macaque",         "Japan", 55),
		new("Mandrill",                 "Southern Cameroon", 110),
		new("Red-shanked Douc",         "Vietnam, Laos", 40),
		new("Gray-shanked Douc",        "Vietnam", 85),
		new("Golden Snub-nosed Monkey", "China", 60),
		new("Black Snub-nosed Monkey",  "China", 95),
		new("Tonkin Snub-nosed Monkey", "Vietnam", 50),
		new("Thomas's Langur",          "Indonesia", 105),
		new("Purple-faced Langur",      "Sri Lanka", 45),
		new("Gelada",                   "Ethiopia", 80),
		new("Proboscis Monkey",         "Borneo", 70),
	];

	readonly CollectionView _collectionView;
	readonly Picker _picker;
	readonly Switch _animateSwitch;

	public Issue34271()
	{
		_picker = new Picker
		{
			AutomationId = "PositionPicker",
			SelectedIndex = 0,
			ItemsSource = new List<string> { "MakeVisible", "Start", "Center", "End" },
		};

		_animateSwitch = new Switch
		{
			AutomationId = "AnimateSwitch",
			IsToggled = false,
		};

		var scrollButton = new Button
		{
			AutomationId = "ScrollButton",
			Text = "Scroll to Proboscis Monkey",
		};

		// Triggers InvalidateMeasure on the CollectionView to force UIKit to
		// recompute layout bounds — an alternative path to reproduce the silent offset reset.
		var triggerLayoutButton = new Button
		{
			AutomationId = "TriggerLayoutButton",
			Text = "Trigger Layout Recomputation",
		};

		_collectionView = new CollectionView
		{
			AutomationId = "MonkeyCollectionView",
			ItemsSource = _monkeys,
			ItemTemplate = new DataTemplate(() =>
			{
				var image = new Image
				{
					// The varying requested heights drive the layout regression, not network loading.
					Source = "dotnet_bot.png",
					Aspect = Aspect.AspectFill,
					WidthRequest = 60,
				};
				image.SetBinding(Image.HeightRequestProperty, nameof(Monkey.ImageHeight));
				Grid.SetRowSpan(image, 2);

				var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
				nameLabel.SetBinding(Label.TextProperty, nameof(Monkey.Name));
				nameLabel.SetBinding(Label.AutomationIdProperty, nameof(Monkey.Name));
				Grid.SetColumn(nameLabel, 1);

				var locationLabel = new Label
				{
					FontAttributes = FontAttributes.Italic,
					VerticalOptions = LayoutOptions.End,
				};
				locationLabel.SetBinding(Label.TextProperty, nameof(Monkey.Location));
				Grid.SetRow(locationLabel, 1);
				Grid.SetColumn(locationLabel, 1);

				var itemGrid = new Grid
				{
					Padding = new Thickness(10),
					RowDefinitions =
					[
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto },
					],
					ColumnDefinitions =
					[
						new ColumnDefinition { Width = GridLength.Auto },
						new ColumnDefinition { Width = GridLength.Star },
					],
				};
				itemGrid.Add(image);
				itemGrid.Add(nameLabel);
				itemGrid.Add(locationLabel);
				return itemGrid;
			}),
		};

		scrollButton.Clicked += (s, e) =>
		{
			var monkey = _monkeys.FirstOrDefault(m => m.Name == "Proboscis Monkey");
			var position = (ScrollToPosition)_picker.SelectedIndex;
			_collectionView.ScrollTo(monkey, position: position, animate: _animateSwitch.IsToggled);
		};

		triggerLayoutButton.Clicked += (s, e) =>
		{
			_collectionView.InvalidateMeasure();
		};

		var pickerRow = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "ScrollToPosition: ", VerticalTextAlignment = TextAlignment.Center },
				_picker,
			},
		};

		var animateRow = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "Animate scroll: ", VerticalTextAlignment = TextAlignment.Center },
				_animateSwitch,
			},
		};

		var grid = new Grid
		{
			Margin = new Thickness(20),
			RowDefinitions =
			[
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			],
		};
		Grid.SetRow(pickerRow, 0);
		Grid.SetRow(animateRow, 1);
		Grid.SetRow(scrollButton, 2);
		Grid.SetRow(triggerLayoutButton, 3);
		Grid.SetRow(_collectionView, 4);
		grid.Children.Add(pickerRow);
		grid.Children.Add(animateRow);
		grid.Children.Add(scrollButton);
		grid.Children.Add(triggerLayoutButton);
		grid.Children.Add(_collectionView);

		Content = grid;
	}
}
