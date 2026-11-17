namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39371,
	"[iOS] Vertical CarouselView does not update Position or raise PositionChanged",
	PlatformAffected.iOS)]
public class Issue39371 : ContentPage
{
	readonly CarouselView _carouselView;
	readonly Label _positionLabel;
	readonly Label _positionChangedLabel;

	public Issue39371()
	{
		_positionLabel = new Label
		{
			AutomationId = "PositionLabel",
			Text = "Position: 0"
		};

		_positionChangedLabel = new Label
		{
			AutomationId = "PositionChangedLabel",
			Text = "PositionChanged: not raised"
		};

		_carouselView = new CarouselView
		{
			AutomationId = "VerticalCarouselView",
			HeightRequest = 300,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			},
			ItemsSource = new[] { "Card 1", "Card 2" },
			Loop = false,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 30,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		_carouselView.PositionChanged += OnPositionChanged;

		var scrollToNextButton = new Button
		{
			AutomationId = "ScrollToNextButton",
			Text = "Scroll to next card"
		};
		scrollToNextButton.Clicked += OnScrollToNextClicked;

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				_positionLabel,
				_positionChangedLabel,
				scrollToNextButton,
				_carouselView
			}
		};
	}

	void OnScrollToNextClicked(object sender, EventArgs e)
	{
		_carouselView.ScrollTo(1, position: ScrollToPosition.Center, animate: false);
	}

	void OnPositionChanged(object sender, PositionChangedEventArgs e)
	{
		_positionLabel.Text = $"Position: {e.CurrentPosition}";
		_positionChangedLabel.Text = $"PositionChanged: {e.CurrentPosition}";
	}
}
