namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14257, "VerticalStackLayout inside Scrollview: Button at the bottom not clickable on IOS", PlatformAffected.iOS)]
	public class Issue14257 : TestContentPage
	{
		protected override void Init()
		{
			var scrollView = new ScrollView();
			var layout = new VerticalStackLayout() { Margin = new Microsoft.Maui.Thickness(10, 40) };

			var description = new Label { Text = "Tap the Resize button; this will force the Test button off the screen. Then tap the Test button; if a Label with the text \"Success\" appears, the test has passed." };

			var resizeButton = new Button() { Text = "Resize", AutomationId = "Resize" };
			var layoutContent = new Label() { Text = "Content", HeightRequest = 50 };
			var testButton = new Button() { Text = "Test", AutomationId = "Test" };
			var resultLabel = new Label() { AutomationId = "Result" };

			layout.Add(description);
			layout.Add(resizeButton);
			layout.Add(layoutContent);
			layout.Add(resultLabel);
			layout.Add(testButton);

			scrollView.Content = layout;
			Content = scrollView;

			resizeButton.Clicked += (sender, args) =>
			{
				// Resize the ScrollView content so the test button will be off the screen
				// If the bug is present, this will make the button untappable
				layoutContent.HeightRequest = 1000;
			};

			// Show the Success label if the button is tapped, so we can verify the bug is not present
			testButton.Clicked += (sender, args) => { resultLabel.Text = "Success"; };
		}
	}
}
