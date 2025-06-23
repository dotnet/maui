using System.Collections.ObjectModel;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, "29245Loop", "Verify tap gestures work correctly with CarouselView Loop", PlatformAffected.UWP)]
public class Issue29245Loop : ContentPage
{
	public Issue29245Loop()
	{
		var verticalStackLayout = new VerticalStackLayout();

		var carouselItems = new ObservableCollection<string>
		{
			"Remain View",
			"Actual View",
			"Percentage View",
		};

		CarouselView carousel = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "CarouselViewLoop",
			HeightRequest = 200,
			Loop = true, // This is the key - issue only occurs when Loop = true
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 10
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
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
			Text = "Change carousel view Position",
			AutomationId = "PositionButton",
			Margin = new Thickness(20),
		};

		// Test label to show if button click worked
		var resultLabel = new Label
		{
			Text = "Button not clicked",
			AutomationId = "ResultLabel",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(20)
		};

		button.Clicked += (sender, e) =>
		{
			carousel.Position = 2;
			resultLabel.Text = "Button clicked successfully!";
		};

		// Add a toggle button to test both loop modes
		var toggleLoopButton = new Button
		{
			Text = "Toggle Loop (currently: True)",
			AutomationId = "ToggleLoopButton",
			Margin = new Thickness(20),
		};

		toggleLoopButton.Clicked += (sender, e) =>
		{
			carousel.Loop = !carousel.Loop;
			toggleLoopButton.Text = $"Toggle Loop (currently: {carousel.Loop})";
			resultLabel.Text = $"Loop toggled to: {carousel.Loop}";
		};

		verticalStackLayout.Children.Add(carousel);
		verticalStackLayout.Children.Add(button);
		verticalStackLayout.Children.Add(toggleLoopButton);
		verticalStackLayout.Children.Add(resultLabel);
		Content = verticalStackLayout;
	}
}