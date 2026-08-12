namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13323,
	"CarouselView on Android does not work if HorizontalTextAlignment in Entry is not Start",
	PlatformAffected.Android)]
public class Issue13323 : ContentPage
{
	public Issue13323()
	{
		var items = Enumerable.Range(0, 5).ToArray();

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
					HeightRequest = 100,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start,
					BackgroundColor = Colors.LightGray,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(Label.TextProperty, ".", stringFormat: "Item {0}");

				var entry = new Entry
				{
					Placeholder = "Tap me",
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};
				entry.SetBinding(AutomationIdProperty, ".", stringFormat: "CenterEntry_{0}");

				return new VerticalStackLayout
				{
					Children = { label, entry }
				};
			})
		};

		var goToItem2Button = new Button
		{
			AutomationId = "GoToItem2",
			Text = "Go to Item 2",
		};
		goToItem2Button.Clicked += (s, e) => carousel.ScrollTo(2, animate: false);

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
					HeightRequest = 100,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Start,
					BackgroundColor = Colors.LightGray,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(Label.TextProperty, ".", stringFormat: "Loop Item {0}");

				var entry = new Entry
				{
					Placeholder = "Tap me",
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};
				entry.SetBinding(AutomationIdProperty, ".", stringFormat: "LoopCenterEntry_{0}");

				return new VerticalStackLayout
				{
					Children = { label, entry }
				};
			})
		};

		var loopGoToItem2Button = new Button
		{
			AutomationId = "LoopGoToItem2",
			Text = "Go to Loop Item 2",
		};
		loopGoToItem2Button.Clicked += (s, e) => loopCarousel.ScrollTo(2, animate: false);

		loopCarousel.PositionChanged += (s, e) =>
		{
			loopPositionLabel.Text = $"LoopPosition:{loopCarousel.Position}";
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Children = { positionLabel, goToItem2Button, carousel, loopPositionLabel, loopGoToItem2Button, loopCarousel }
			}
		};
	}
}
