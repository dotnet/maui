namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34271, "CollectionView scroll position resets to top after ScrollTo last item when Picker is dismissed", PlatformAffected.macOS, isInternetRequired: true)]
public class Issue34271 : ContentPage
{
	// Varying image heights widen the gap between UIKit's estimated contentSize
	// (NSCollectionLayoutDimension.CreateEstimated(30f)) and actual contentSize,
	// making the silent offset reset more pronounced and reliably testable.
	public record Monkey(string Name, string Location, string ImageUrl, double ImageHeight);

	readonly List<Monkey> _monkeys =
	[
		new("Baboon",                   "Africa & Asia",        "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg", 40),
		new("Capuchin Monkey",          "Central & South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/Capuchin_Costa_Rica.jpg/200px-Capuchin_Costa_Rica.jpg", 80),
		new("Blue Monkey",              "Central and East Africa", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/BlueMonkey.jpg/220px-BlueMonkey.jpg", 50),
		new("Squirrel Monkey",          "Central & South America", "https://upload.wikimedia.org/wikipedia/commons/thumb/2/20/Saimiri_sciureus-1_Luc_Viatour.jpg/220px-Saimiri_sciureus-1_Luc_Viatour.jpg", 100),
		new("Golden Lion Tamarin",      "Brazil",               "https://upload.wikimedia.org/wikipedia/commons/thumb/8/87/Golden_lion_tamarin_portrait3.jpg/220px-Golden_lion_tamarin_portrait3.jpg", 45),
		new("Howler Monkey",            "South America",        "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0d/Alouatta_guariba.jpg/200px-Alouatta_guariba.jpg", 90),
		new("Japanese Macaque",         "Japan",                "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Macaca_fuscata_fuscata1.jpg/220px-Macaca_fuscata_fuscata1.jpg", 55),
		new("Mandrill",                 "Southern Cameroon",    "https://upload.wikimedia.org/wikipedia/commons/thumb/7/75/Mandrill_at_san_francisco_zoo.jpg/220px-Mandrill_at_san_francisco_zoo.jpg", 110),
		new("Red-shanked Douc",         "Vietnam, Laos",        "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9f/Portrait_of_a_Douc.jpg/159px-Portrait_of_a_Douc.jpg", 40),
		new("Gray-shanked Douc",        "Vietnam",              "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/Cuc.Phuong.Primate.Rehab.center.jpg/320px-Cuc.Phuong.Primate.Rehab.center.jpg", 85),
		new("Golden Snub-nosed Monkey", "China",                "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c8/Golden_Snub-nosed_Monkeys%2C_Qinling_Mountains_-_China.jpg/165px-Golden_Snub-nosed_Monkeys%2C_Qinling_Mountains_-_China.jpg", 60),
		new("Black Snub-nosed Monkey",  "China",                "https://upload.wikimedia.org/wikipedia/commons/thumb/5/59/RhinopitecusBieti.jpg/320px-RhinopitecusBieti.jpg", 95),
		new("Tonkin Snub-nosed Monkey", "Vietnam",              "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9c/Tonkin_snub-nosed_monkeys_%28Rhinopithecus_avunculus%29.jpg/320px-Tonkin_snub-nosed_monkeys_%28Rhinopithecus_avunculus%29.jpg", 50),
		new("Thomas's Langur",          "Indonesia",            "https://upload.wikimedia.org/wikipedia/commons/thumb/3/31/Thomas%27s_langur_Presbytis_thomasi.jpg/142px-Thomas%27s_langur_Presbytis_thomasi.jpg", 105),
		new("Purple-faced Langur",      "Sri Lanka",            "https://upload.wikimedia.org/wikipedia/commons/thumb/0/02/Semnopith%C3%A8que_blanch%C3%A2tre_m%C3%A2le.JPG/192px-Semnopith%C3%A8que_blanch%C3%A2tre_m%C3%A2le.JPG", 45),
		new("Gelada",                   "Ethiopia",             "https://upload.wikimedia.org/wikipedia/commons/thumb/1/13/Gelada-Pavian.jpg/320px-Gelada-Pavian.jpg", 80),
		new("Proboscis Monkey",         "Borneo",               "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Proboscis_Monkey_in_Borneo.jpg/250px-Proboscis_Monkey_in_Borneo.jpg", 70),
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
					Aspect = Aspect.AspectFill,
					WidthRequest = 60,
				};
				image.SetBinding(Image.SourceProperty, nameof(Monkey.ImageUrl));
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
