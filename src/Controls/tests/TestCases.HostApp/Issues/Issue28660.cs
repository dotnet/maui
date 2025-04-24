namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28660, "Label text gets cropped when a width request is specified on the label inside a VerticalStackLayout", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue28660 : ContentPage
{
	public Issue28660()
	{
		Content = new VerticalStackLayout
		{
			WidthRequest = 350,
			Children =
			{
				new Label
				{
					Text="At any time, but not later than one month before the expiration date.",
					WidthRequest=100.94,
					FontSize = 16,
					BackgroundColor = Colors.Pink,
					AutomationId = "Label",
				},
				new Label
				{
					Text="At any time, but not later than one month before the expiration date.",
					WidthRequest=100.94,
					FontSize = 16,
					AutomationId = "Label",
				}
			}
		};
	}
}