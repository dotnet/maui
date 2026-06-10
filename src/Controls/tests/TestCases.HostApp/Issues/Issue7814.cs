namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7814, "Vertical scrolling not working for CarouselView and CustomLayouts", PlatformAffected.Android)]
public class Issue7814 : TestContentPage
{
	const string VerticalOffsetPrefix = "VerticalScrollY";
	const string HorizontalOffsetPrefix = "HorizontalScrollX";

	Label _verticalOffsetLabel = null!;
	Label _horizontalOffsetLabel = null!;

	protected override void Init()
	{
		_verticalOffsetLabel = CreateOffsetLabel("Issue7814VerticalScrollYLabel", VerticalOffsetPrefix);
		_horizontalOffsetLabel = CreateOffsetLabel("Issue7814HorizontalScrollXLabel", HorizontalOffsetPrefix);

		Grid.SetColumn(_horizontalOffsetLabel, 1);

		var outerScrollView = new ScrollView
		{
			AutomationId = "Issue7814OuterScrollView",
			Orientation = ScrollOrientation.Vertical,
			Content = CreateScrollableContent()
		};
		outerScrollView.Scrolled += (_, e) => UpdateOffset(_verticalOffsetLabel, VerticalOffsetPrefix, e.ScrollY);
		Grid.SetRow(outerScrollView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				new Grid
				{
					Padding = new Thickness(12, 8),
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Star)
					},
					Children =
					{
						_verticalOffsetLabel,
						_horizontalOffsetLabel
					}
				},
				outerScrollView
			}
		};
	}

	View CreateScrollableContent()
	{
		var carouselView = new CarouselView
		{
			AutomationId = "Issue7814CarouselView",
			HeightRequest = 420,
			Loop = false,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemsSource = Enumerable.Range(1, 5).Select(index => $"Carousel item {index}").ToList(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 24,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Border
				{
					Margin = new Thickness(12),
					BackgroundColor = Colors.YellowGreen,
					Stroke = Colors.Green,
					StrokeThickness = 2,
					Content = label
				};
			})
		};

		var horizontalItems = new HorizontalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(12, 0)
		};

		foreach (var index in Enumerable.Range(1, 8))
		{
			horizontalItems.Children.Add(new Border
			{
				WidthRequest = 330,
				HeightRequest = 300,
				Stroke = Colors.Green,
				StrokeThickness = 2,
				Content = new Label
				{
					Text = $"Horizontal item {index}",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				}
			});
		}

		var horizontalScrollView = new ScrollView
		{
			AutomationId = "Issue7814HorizontalScrollView",
			Orientation = ScrollOrientation.Horizontal,
			HeightRequest = 220,
			Content = horizontalItems
		};
		horizontalScrollView.Scrolled += (_, e) => UpdateOffset(_horizontalOffsetLabel, HorizontalOffsetPrefix, e.ScrollX);

		return new VerticalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(0, 12),
			Children =
			{
				new Label
				{
					Text = "Touch and scroll vertically from the carousel, then horizontally from the list below.",
					Margin = new Thickness(12, 0)
				},
				carouselView,
				new Label
				{
					Text = "Horizontal ScrollView",
					FontSize = 20,
					Margin = new Thickness(12, 0)
				},
				horizontalScrollView,
				new BoxView
				{
					HeightRequest = 900,
					Color = Colors.Transparent
				}
			}
		};
	}

	static Label CreateOffsetLabel(string automationId, string prefix)
	{
		var label = new Label
		{
			AutomationId = automationId,
			FontSize = 12,
			Text = $"{prefix}: 0"
		};

		return label;
	}

	static void UpdateOffset(Label label, string prefix, double offset)
	{
		label.Text = $"{prefix}: {(int)Math.Round(offset)}";
	}
}
