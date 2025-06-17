using Maui.Controls.Sample.Issues;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 29216, "Carousel view scrolling on button click", PlatformAffected.UWP)]
public class Issue29216 : TestContentPage
{
	protected override void Init()
	{
		var items = new List<string> { "Page1", "Page2" };

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = items,
			IsSwipeEnabled = false,
			Loop = false,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 24,
					TextColor = Colors.Black,
				};

				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				return new StackLayout
				{
					Children = { label }
				};
			}),
			HeightRequest = 300
		};

		var button1 = new Button
		{
			Text = "Go to Page 2",
			AutomationId = "button"
		};

		button1.Clicked += (s, e) => carouselView.Position = 1;

		Content = new StackLayout
		{
			Padding = 20,
			Children =
			{
				new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Children = { button1 }
				},
				carouselView
			}
		};
	}
}
