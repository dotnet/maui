using System.Collections;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27110_1, "ScrollTo a CollectionView item by object", PlatformAffected.UWP)]
public sealed class Issue27110_1 : Issue27110ObjectScrollSamplePage
{
	readonly IList<Issue27110Monkey> _monkeys;

	public Issue27110_1()
		: this(Issue27110ObjectScrollSampleData.CreateMonkeys())
	{
	}

	Issue27110_1(IList<Issue27110Monkey> monkeys)
		: base("Scroll to object", monkeys, isGrouped: false)
	{
		_monkeys = monkeys;
	}

	protected override void ScrollToItem(ScrollToPosition position, bool animate)
	{
		var monkey = _monkeys.FirstOrDefault(item => item.Name == "Proboscis Monkey")
			?? throw new InvalidOperationException("The Proboscis Monkey item is missing from the sample data.");
		CollectionView.ScrollTo(monkey, position: position, animate: animate);
	}
}

public abstract class Issue27110ObjectScrollSamplePage : ContentPage
{
	ScrollToPosition _scrollToPosition = ScrollToPosition.MakeVisible;
	readonly Switch _animateSwitch;
	readonly Label _scrollOffsetLabel;

	internal CollectionView2 CollectionView { get; }

	protected Issue27110ObjectScrollSamplePage(string title, IEnumerable itemsSource, bool isGrouped)
	{
		Title = title;

		var instructions = new Label
		{
			Text = "Scroll to the Proboscis Monkey using MakeVisible, then select Start, Center, and End and scroll again."
		};
		var positionSelector = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "ScrollToPosition: ", VerticalTextAlignment = TextAlignment.Center },
				CreatePositionButton(ScrollToPosition.Start),
				CreatePositionButton(ScrollToPosition.Center),
				CreatePositionButton(ScrollToPosition.End)
			}
		};
		_animateSwitch = new Switch
		{
			AutomationId = "AnimateScrollSwitch",
			IsToggled = false
		};
		var animationSelector = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "Animate scroll: ", VerticalTextAlignment = TextAlignment.Center },
				_animateSwitch
			}
		};
		var scrollButton = new Button
		{
			Text = "Scroll to Proboscis Monkey",
			AutomationId = "ScrollToProboscisMonkeyButton"
		};
		scrollButton.Clicked += OnScrollButtonClicked;
		_scrollOffsetLabel = new Label
		{
			AutomationId = "ScrollOffsetLabel",
			Text = "0",
			VerticalTextAlignment = TextAlignment.Center
		};
		var scrollAction = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				scrollButton,
				new Label { Text = "Vertical offset:", VerticalTextAlignment = TextAlignment.Center },
				_scrollOffsetLabel
			}
		};

		CollectionView = new CollectionView2
		{
			AutomationId = "CollectionView",
			HeightRequest = 360,
			ItemsSource = itemsSource,
			IsGrouped = isGrouped,
			ItemTemplate = new DataTemplate(CreateItemTemplate),
			GroupHeaderTemplate = isGrouped ? new DataTemplate(CreateGroupHeaderTemplate) : null,
			GroupFooterTemplate = isGrouped ? new DataTemplate(CreateGroupFooterTemplate) : null
		};
		CollectionView.Scrolled += OnCollectionViewScrolled;

		var layout = new Grid
		{
			Margin = 20,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			Children = { instructions, positionSelector, animationSelector, scrollAction, CollectionView }
		};
		Grid.SetRow(instructions, 0);
		Grid.SetRow(positionSelector, 1);
		Grid.SetRow(animationSelector, 2);
		Grid.SetRow(scrollAction, 3);
		Grid.SetRow(CollectionView, 4);
		Content = layout;
	}

	protected abstract void ScrollToItem(ScrollToPosition position, bool animate);

	Button CreatePositionButton(ScrollToPosition position)
	{
		var button = new Button
		{
			Text = position.ToString(),
			AutomationId = $"Select{position}Button"
		};
		button.Clicked += (_, _) => _scrollToPosition = position;
		return button;
	}

	void OnScrollButtonClicked(object sender, EventArgs e)
	{
		ScrollToItem(_scrollToPosition, _animateSwitch.IsToggled);
	}

	void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		_scrollOffsetLabel.Text = e.VerticalOffset.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
	}

	static View CreateItemTemplate()
	{
		var item = new Grid
		{
			Padding = 10,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Star }
			}
		};
		var placeholder = new BoxView
		{
			Color = Colors.LightGray,
			HeightRequest = 60,
			WidthRequest = 60
		};
		Grid.SetRowSpan(placeholder, 2);
		item.Children.Add(placeholder);

		var name = new Label { FontAttributes = FontAttributes.Bold };
		name.SetBinding(Label.TextProperty, nameof(Issue27110Monkey.Name));
		name.SetBinding(Label.AutomationIdProperty, nameof(Issue27110Monkey.Name));
		Grid.SetColumn(name, 1);
		item.Children.Add(name);

		var location = new Label
		{
			FontAttributes = FontAttributes.Italic,
			VerticalOptions = LayoutOptions.End
		};
		location.SetBinding(Label.TextProperty, nameof(Issue27110Monkey.Location));
		Grid.SetRow(location, 1);
		Grid.SetColumn(location, 1);
		item.Children.Add(location);

		return item;
	}

	static View CreateGroupHeaderTemplate()
	{
		var header = new Label
		{
			BackgroundColor = Colors.LightGray,
			FontAttributes = FontAttributes.Bold,
			FontSize = 20
		};
		header.SetBinding(Label.TextProperty, nameof(Issue27110MonkeyGroup.Name));
		return header;
	}

	static View CreateGroupFooterTemplate()
	{
		var footer = new Label { Margin = new Thickness(0, 0, 0, 10) };
		footer.SetBinding(
			Label.TextProperty,
			new Binding(nameof(Issue27110MonkeyGroup.Count), stringFormat: "Total animals: {0:D}"));
		return footer;
	}
}

public sealed class Issue27110MonkeyGroup : List<Issue27110Monkey>
{
	public Issue27110MonkeyGroup(string name, IEnumerable<Issue27110Monkey> monkeys)
		: base(monkeys)
	{
		Name = name;
	}

	public string Name { get; }
}

static class Issue27110ObjectScrollSampleData
{
	public static IList<Issue27110Monkey> CreateMonkeys() =>
	[
		new("Baboon", "Africa & Asia"),
		new("Capuchin Monkey", "Central & South America"),
		new("Blue Monkey", "Central and East Africa"),
		new("Squirrel Monkey", "Central & South America"),
		new("Golden Lion Tamarin", "Brazil"),
		new("Howler Monkey", "South America"),
		new("Japanese Macaque", "Japan"),
		new("Mandrill", "Central Africa"),
		new("Proboscis Monkey", "Borneo"),
		new("Red-shanked Douc", "Vietnam, Laos"),
		new("Gray-shanked Douc", "Vietnam"),
		new("Golden Snub-nosed Monkey", "China")
	];

	public static IList<Issue27110MonkeyGroup> CreateGroups() =>
	[
		CreateGroup("Bears", "American Black Bear", "Asian Black Bear", "Brown Bear", "Polar Bear"),
		CreateGroup("Cats", "Lion", "Tiger", "Cheetah", "Leopard"),
		CreateGroup("Dogs", "Gray Wolf", "Red Fox", "Coyote", "Dhole"),
		CreateGroup("Elephants", "African Elephant", "Asian Elephant", "Forest Elephant"),
		CreateGroup("Monkeys",
			"Baboon", "Capuchin Monkey", "Blue Monkey", "Squirrel Monkey", "Golden Lion Tamarin",
			"Howler Monkey", "Japanese Macaque", "Mandrill", "Proboscis Monkey", "Red-shanked Douc",
			"Gray-shanked Douc", "Golden Snub-nosed Monkey")
	];

	static Issue27110MonkeyGroup CreateGroup(string name, params string[] monkeyNames) =>
		new(name, monkeyNames.Select(monkeyName => new Issue27110Monkey(monkeyName, "Various habitats")));
}
