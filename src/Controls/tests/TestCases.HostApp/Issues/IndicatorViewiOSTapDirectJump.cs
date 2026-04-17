using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(
		IssueTracker.Github,
		27007,
		"iOS IndicatorView dot tap only advances by plus/minus 1 instead of jumping directly to the tapped dot",
		PlatformAffected.iOS
	)]
	public class IndicatorViewiOSTapDirectJump : ContentPage
	{
		const string CarouselId = "jumpCarousel";
		const string IndicatorId = "jumpIndicator";
		const string PositionLabelId = "jumpPositionLabel";

		readonly CarouselView _carousel;
		readonly IndicatorView _indicator;
		readonly Label _positionLabel;

		public IndicatorViewiOSTapDirectJump()
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
							"Repro: tapping the rightmost indicator dot must jump directly to the last item, not advance by 1.",
					},
					_positionLabel,
					_carousel,
					_indicator,
				},
			};
		}
	}
}
