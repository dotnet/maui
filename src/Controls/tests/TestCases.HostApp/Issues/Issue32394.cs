namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32394, "CurrentItem Should not update on Orientation Change", PlatformAffected.iOS)]
public class Issue32394 : ContentPage
{
	CarouselView2 carouselView;
	Label currentItemLabel;
	public Issue32394()
	{
		carouselView = new CarouselView2
		{
			AutomationId = "Issue32394CarouselView",
			HeightRequest = 150,
			BackgroundColor = Colors.LightGray,
			Loop = false,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				label.SetBinding(Label.TextProperty, ".");

				return new Grid
				{
					Children = { label }
				};
			}),
			ItemsSource = new string[]
			{
				"Baboon",
				"Capuchin Monkey",
				"Blue Monkey",
				"Squirrel Monkey",
				"Golden Lion Tamarin"
			}
		};

		currentItemLabel = new Label
		{
			AutomationId = "Issue32394StatusLabel",
			Text = "Test passes if current item not changed on orientation change"
		};

		Button button = new Button
		{
			AutomationId = "Issue32394SetPositionButton",
			Text = "set position",
			Command = new Command(() =>
			{
				carouselView.Position = 3;
			})
		};

		VerticalStackLayout verticalLayout = new VerticalStackLayout
		{
			Children =
			{
				carouselView,
				button,
				currentItemLabel
			}
		};

		Content = verticalLayout;
	}
}