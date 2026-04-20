using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(
		IssueTracker.Github,
		27007,
		"IndicatorView dot tap only advances by plus/minus 1 instead of jumping directly to the tapped dot",
		PlatformAffected.iOS | PlatformAffected.macOS
	)]
	public class IndicatorViewTapDirectJump : ContentPage
	{
		const string CarouselId = "jumpCarousel";
		const string IndicatorId = "jumpIndicator";
		const string PositionLabelId = "jumpPositionLabel";

		readonly CarouselView _carousel;
		readonly IndicatorView _indicator;
		readonly Label _positionLabel;

		public IndicatorViewTapDirectJump()
		{
			var items = new ObservableCollection<string>(
				Enumerable.Range(0, 5).Select(i => $"Item {i}")
			);

			_carousel = new CarouselView
			{
				AutomationId = CarouselId,
				ItemsSource = items,
				Loop = false,
				HeightRequest = 300,
				BackgroundColor = Colors.LightYellow,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 32,
					};
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
			};

			_indicator = new IndicatorView
			{
				AutomationId = IndicatorId,
				IndicatorColor = Colors.Gray,
				SelectedIndicatorColor = Colors.Red,
				IndicatorsShape = IndicatorShape.Circle,
				IndicatorSize = 20,
				MaximumVisible = 10,
				HorizontalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.White,
				HeightRequest = 40,
			};

			_carousel.IndicatorView = _indicator;

			_positionLabel = new Label
			{
				Text = "Pos:0",
				AutomationId = PositionLabelId,
				FontSize = 20,
			};

			_carousel.PositionChanged += (s, e) =>
			{
				_positionLabel.Text = $"Pos:{e.CurrentPosition}";
			};

			Content = new VerticalStackLayout
			{
				Spacing = 4,
				Padding = 8,
				Children =
				{
					new Label
					{
						Text =
							"Tapping the rightmost indicator dot must jump directly to the last item. On iOS/Catalyst this currently advances by only 1 (bug #27007).",
					},
					_positionLabel,
					_carousel,
					_indicator,
				},
			};
		}
	}
}
