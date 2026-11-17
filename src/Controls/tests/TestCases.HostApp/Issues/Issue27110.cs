using System.Globalization;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27110, "Subsequent non animated CollectionView scroll requests stop working on Windows", PlatformAffected.UWP)]
public class Issue27110 : ContentPage
{
	ScrollToPosition _scrollToPosition = ScrollToPosition.MakeVisible;
	readonly Switch _animateSwitch;
	readonly CollectionView2 _collectionView;
	readonly Label _scrollOffsetLabel;

	public IList<Issue27110Monkey> Monkeys { get; }

	public Issue27110()
	{
		Monkeys = CreateMonkeys();

		var instructions = new ScrollView
		{
			HeightRequest = 60,
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Scroll to the Proboscis Monkey with MakeVisible, Start, Center, and End. Turn off Animate scroll and repeat the requests."
					}
				}
			}
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
		scrollButton.Clicked += OnButtonClicked;
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

		_collectionView = new CollectionView2
		{
			AutomationId = "CollectionView",
			HeightRequest = 360,
			ItemsSource = Monkeys,
			ItemTemplate = new DataTemplate(CreateItemTemplate)
		};
		_collectionView.Scrolled += OnCollectionViewScrolled;

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
			Children =
			{
				instructions,
				positionSelector,
				animationSelector,
				scrollAction,
				_collectionView
			}
		};
		Grid.SetRow(instructions, 0);
		Grid.SetRow(positionSelector, 1);
		Grid.SetRow(animationSelector, 2);
		Grid.SetRow(scrollAction, 3);
		Grid.SetRow(_collectionView, 4);
		Content = layout;
	}

	void OnButtonClicked(object sender, EventArgs e)
	{
		var monkey = Monkeys.FirstOrDefault(item => item.Name == "Proboscis Monkey");
		_collectionView.ScrollTo(monkey, position: _scrollToPosition, animate: _animateSwitch.IsToggled);
	}

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

	void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		_scrollOffsetLabel.Text = e.VerticalOffset.ToString("R", CultureInfo.InvariantCulture);
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

	static IList<Issue27110Monkey> CreateMonkeys()
	{
		return new List<Issue27110Monkey>
		{
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
			new("Golden Snub-nosed Monkey", "China"),
			new("Black-and-white Colobus", "Africa"),
			new("Vervet Monkey", "Africa"),
			new("Spider Monkey", "Central and South America"),
			new("Tamarin", "South America"),
			new("Marmoset", "South America"),
			new("Macaque", "Asia")
		};
	}
}

public sealed record Issue27110Monkey(string Name, string Location);
