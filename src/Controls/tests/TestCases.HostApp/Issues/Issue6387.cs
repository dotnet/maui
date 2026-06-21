namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6387, "ArgumentException thrown when a negative value is set for the padding of a label", PlatformAffected.UWP)]
public class Issue6387 : ContentPage
{
	public Issue6387()
	{
		Label labelWithNegativePaddingValue = new Label
		{
			AutomationId = "LabelWithNegativePaddingValue",
			Text = "The test passes if the app runs without crashing and fails if the app crashes",
			Padding = new Thickness(-2)
		};

		VerticalStackLayout verticalStackLayout = new VerticalStackLayout
		{
			Padding = 20,
			Children = { labelWithNegativePaddingValue }
		};

		Content = verticalStackLayout;
	}
}
