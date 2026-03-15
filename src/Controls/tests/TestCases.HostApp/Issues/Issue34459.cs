namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34459, "Android Label word wrapping clips text depending on alignment and layout options", PlatformAffected.Android)]
public class Issue34459 : ContentPage
{
	// Exact sandbox reproduction:
	// A RightToLeft VerticalStackLayout (WidthRequest=150) is the direct page Content.
	// The WordWrap label inside clips text on Android when VerticalOptions/HorizontalOptions
	// are set — only single characters ("H", "W") remain visible.
	public Issue34459()
	{
		var rtlLabel = new Label
		{
			Text = "Hello, World!",
			VerticalOptions = LayoutOptions.Start,
			HorizontalOptions = LayoutOptions.Start,
			HorizontalTextAlignment = TextAlignment.Start,
			LineBreakMode = LineBreakMode.WordWrap,
			FontSize = 32,
			AutomationId = "RtlLabel",
		};

		// Match the sandbox layout exactly: RTL VerticalStackLayout as the page Content.
		Content = new VerticalStackLayout
		{
			BackgroundColor = Colors.Gray,
			FlowDirection = FlowDirection.RightToLeft,
			WidthRequest = 150,
			AutomationId = "RtlContainer",
			Children = { rtlLabel },
		};
	}
}
