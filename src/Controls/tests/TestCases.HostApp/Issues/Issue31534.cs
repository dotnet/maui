namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31534, "[WinUI] ScrollView height was increased after the application closes", PlatformAffected.UWP)]
public class Issue31534 : ContentPage
{
	public Issue31534()
	{
		// Create ScrollView
		var scrollView = new ScrollView
		{
			BackgroundColor = Colors.LightGray
		};

		// Create layout inside ScrollView
		var stackLayout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10
		};

		var HorizontalLayout = new HorizontalStackLayout
		{
			Spacing = 10
		};

		var heightLabel = new Label
		{
			Text = "ScrollView Height:",
			FontSize = 16,
			TextColor = Colors.Red
		};

		var valueLabel = new Label
		{
			Text = "0",
			FontSize = 16,
			TextColor = Colors.Red,
			AutomationId = "HeightLabel"
		};

		HorizontalLayout.Add(heightLabel);
		HorizontalLayout.Add(valueLabel);
		stackLayout.Add(HorizontalLayout);

		for (int i = 1; i <= 20; i++)
		{
			stackLayout.Add(new Label
			{
				Text = $"Item {i}"
			});
		}

		// Put layout inside ScrollView
		scrollView.Content = stackLayout;

		// Set page content
		Content = scrollView;

		// Attach SizeChanged handler
		scrollView.SizeChanged += (s, e) =>
		{
			valueLabel.Text = scrollView.Height.ToString();
		};
	}
}