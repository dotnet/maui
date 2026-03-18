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
			ItemTemplate = new DataTemplate(() =>
			{
				var entry = new Entry
				{
					AutomationId = "CenterEntry",
					Placeholder = "Tap me",
					HorizontalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};

				return new Border
				{
					HeightRequest = 200,
					Content = new VerticalStackLayout
					{
						VerticalOptions = LayoutOptions.Center,
						Children = { entry }
					}
				};
			})
		};

		carousel.PositionChanged += (s, e) =>
		{
			positionLabel.Text = $"Position:{carousel.Position}";
		};

		Content = new VerticalStackLayout
		{
			Children = { positionLabel, carousel }
		};
	}
}
