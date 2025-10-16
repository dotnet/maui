namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14200, "Vertical Stack Layout compressed in IOS and MacCatalyst", PlatformAffected.iOS)]
public class Issue14200 : ContentPage
{
	public Issue14200()
	{
		var verticalStack = new VerticalStackLayout
		{
			WidthRequest = 200,
			BackgroundColor = Colors.LightGray,
			Children =
			{
				new Label
				{
					AutomationId = "Issue14200Label",
					LineBreakMode = LineBreakMode.WordWrap,
					Text = "This is a test to see if the text is fully visible in iOS and MacCatalyst. The text should wrap and be fully visible.",
				}
			}
		};
		Content = verticalStack;
	}
}
