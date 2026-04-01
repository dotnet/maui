namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13323,
	"CarouselView on Android does not work if HorizontalTextAlignment in Entry is not Start",
	PlatformAffected.Android)]
public class Issue13323 : ContentPage
{
	public Issue13323()
	{
		var items = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };

		var positionLabel = new Label
		{
			AutomationId = "PositionLabel",
			Text = "Position:0",
			HorizontalOptions = LayoutOptions.Center,
		};

		var carousel = new CarouselView
		{
			AutomationId = "CarouselView13323",
			ItemsSource = items,
			Loop = false,
			HeightRequest = 250,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					AutomationId = "SwipeArea",
					HeightRequest = 100,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start,
					BackgroundColor = Colors.LightGray,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(Label.TextProperty, ".");

				var entry = new Entry
				{
					AutomationId = "CenterEntry",
					Placeholder = "Tap me",
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};

				return new VerticalStackLayout
				{
					Children = { label, entry }
				};
			})
		};

		carousel.PositionChanged += (s, e) =>
		{
			positionLabel.Text = $"Position:{carousel.Position}";
		};

		var loopPositionLabel = new Label
		{
			AutomationId = "LoopPositionLabel",
			Text = "LoopPosition:0",
			HorizontalOptions = LayoutOptions.Center,
		};

		var loopCarousel = new CarouselView
		{
			AutomationId = "LoopCarouselView13323",
			ItemsSource = items,
			Loop = true,
			HeightRequest = 250,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					AutomationId = "LoopSwipeArea",
					HeightRequest = 100,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start,
					BackgroundColor = Colors.LightGray,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(Label.TextProperty, ".");

				var entry = new Entry
				{
					AutomationId = "LoopCenterEntry",
					Placeholder = "Tap me",
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};

				return new VerticalStackLayout
				{
					Children = { label, entry }
				};
			})
		};

		loopCarousel.PositionChanged += (s, e) =>
		{
			loopPositionLabel.Text = $"LoopPosition:{loopCarousel.Position}";
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Children = { positionLabel, carousel, loopPositionLabel, loopCarousel }
			}
		};
	}
}
