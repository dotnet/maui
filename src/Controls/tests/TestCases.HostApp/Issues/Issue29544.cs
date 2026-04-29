using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29544,
	"PreviousItem and PreviousPosition not updating correctly on ScrollTo or Position set",
	PlatformAffected.Android | PlatformAffected.UWP)]
public class Issue29544 : ContentPage
{
	readonly CarouselView _carouselView;
	readonly ObservableCollection<string> _items;
	readonly Label _previousItemLabel;
	readonly Label _previousPositionLabel;
	readonly Label _currentItemLabel;
	readonly Label _currentPositionLabel;

	public Issue29544()
	{
		_items = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
			"Item 5"
		};

		_currentItemLabel = new Label
		{
			AutomationId = "CurrentItemLabel",
			Text = "Current Item: Item 1"
		};

		_previousItemLabel = new Label
		{
			AutomationId = "PreviousItemLabel",
			Text = "Previous Item: none"
		};

		_currentPositionLabel = new Label
		{
			AutomationId = "CurrentPositionLabel",
			Text = "Current Position: 0"
		};

		_previousPositionLabel = new Label
		{
			AutomationId = "PreviousPositionLabel",
			Text = "Previous Position: none"
		};

		_carouselView = new CarouselView2
		{
			AutomationId = "CarouselView",
			Loop = false,
			ItemsSource = _items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 24,
					BackgroundColor = Colors.LightGray,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		_carouselView.CurrentItemChanged += OnCurrentItemChanged;
		_carouselView.PositionChanged += OnPositionChanged;

		var scrollTo3Button = new Button
		{
			AutomationId = "ScrollTo3Button",
			Text = "Scroll To 3"
		};
		scrollTo3Button.Clicked += (s, e) => _carouselView.ScrollTo(3);

		var scrollTo1Button = new Button
		{
			AutomationId = "ScrollTo1Button",
			Text = "Scroll To 1"
		};
		scrollTo1Button.Clicked += (s, e) => _carouselView.ScrollTo(1);

		var setPosition3Button = new Button
		{
			AutomationId = "SetPosition3Button",
			Text = "Set Position 3"
		};
		setPosition3Button.Clicked += (s, e) => _carouselView.Position = 3;

		var setPosition0Button = new Button
		{
			AutomationId = "SetPosition0Button",
			Text = "Set Position 0"
		};
		setPosition0Button.Clicked += (s, e) => _carouselView.Position = 0;

		Content = new Grid
		{
			Padding = new Thickness(10),
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			Children =
			{
				new VerticalStackLayout
				{
					Spacing = 6,
					Children =
					{
						_currentItemLabel,
						_previousItemLabel,
						_currentPositionLabel,
						_previousPositionLabel,
						new HorizontalStackLayout
						{
							Spacing = 8,
							Children = { scrollTo3Button, scrollTo1Button }
						},
						new HorizontalStackLayout
						{
							Spacing = 8,
							Children = { setPosition3Button, setPosition0Button }
						}
					}
				},
				_carouselView
			}
		};

		Grid.SetRow(_carouselView, 1);
	}

	void OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
	{
		_currentItemLabel.Text = $"Current Item: {e.CurrentItem}";
		_previousItemLabel.Text = $"Previous Item: {e.PreviousItem ?? "none"}";
	}

	void OnPositionChanged(object sender, PositionChangedEventArgs e)
	{
		_currentPositionLabel.Text = $"Current Position: {e.CurrentPosition}";
		_previousPositionLabel.Text = $"Previous Position: {e.PreviousPosition}";
	}
}
