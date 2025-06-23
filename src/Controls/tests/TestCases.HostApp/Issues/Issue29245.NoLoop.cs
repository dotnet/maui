using System.Collections.ObjectModel;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, "29245NoLoop", "Verify tap gestures work correctly with CarouselView no Loop", PlatformAffected.UWP)]
public class Issue29245NoLoop : ContentPage
{
	public Issue29245NoLoop()
	{
		var verticalStackLayout = new VerticalStackLayout();

		var carouselItems = new ObservableCollection<string>
		{
			"First View",
			"Second View",
			"Third View",
		};

		CarouselView carousel = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "CarouselViewNoLoop",
			HeightRequest = 200,
			Loop = false, // Explicitly set to false to test this scenario
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 10,
					BackgroundColor = Colors.LightBlue
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
					TextColor = Colors.Black
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				grid.Children.Add(label);
				return grid;
			}),
			HorizontalOptions = LayoutOptions.Fill,
		};

		var button = new Button
		{
			Text = "Test Button (No Loop)",
			AutomationId = "NoLoopButton",
			Margin = new Thickness(20),
			BackgroundColor = Colors.Green,
			TextColor = Colors.White
		};

		// Test label to show if button click worked
		var resultLabel = new Label
		{
			Text = "No Loop: Button not clicked",
			AutomationId = "NoLoopResultLabel",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(20),
			FontAttributes = FontAttributes.Bold
		};

		button.Clicked += (sender, e) =>
		{
			carousel.Position = 1;
			resultLabel.Text = "No Loop: Button clicked successfully!";
		};

		verticalStackLayout.Children.Add(new Label
		{
			Text = "CarouselView with Loop = False",
			FontSize = 16,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(10)
		});
		verticalStackLayout.Children.Add(carousel);
		verticalStackLayout.Children.Add(button);
		verticalStackLayout.Children.Add(resultLabel);
		Content = verticalStackLayout;
	}
}