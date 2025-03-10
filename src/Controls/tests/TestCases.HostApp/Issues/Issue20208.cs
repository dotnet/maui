using Maui.Controls.Sample.Issues;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 20208, "When minimumheightrequest is set on label, the label is not vertically centered in Windows", PlatformAffected.UWP)]
public class Issue20208 : TestContentPage
{
	protected override void Init()
	{
		FlexLayout flexLayout = new FlexLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.Red,
			HeightRequest = 50,
		};

		Label labelWithMinimumHeight = new Label
		{
			AutomationId = "LabelWithMinimumHeight",
			VerticalOptions = LayoutOptions.Fill,
			VerticalTextAlignment = TextAlignment.Center,
			MinimumHeightRequest = 39,
			Text = "Hello, World!"
		};

		flexLayout.Children.Add(labelWithMinimumHeight);

		Content = flexLayout;
	}
}
