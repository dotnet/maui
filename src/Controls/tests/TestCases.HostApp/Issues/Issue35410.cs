namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35410, "iOS landscape rotation causes ScrollView content to be obscured by device notch", PlatformAffected.iOS)]
public class Issue35410 : ContentPage
{
	public Issue35410()
	{
		// Reproduce the exact hierarchy from the bug report:
		// ContentPage > ScrollView > VerticalStackLayout > Editor
		// The ScrollView uses the default SafeAreaEdges (SafeAreaRegions.Default → CIAB.Automatic).
		// Before the fix, rotating to landscape-left caused _safeArea.Left = 0 (from
		// SystemAdjustedContentInset which excludes horizontal edges for vertical scroll views),
		// so CrossPlatformArrange placed the content at x=0, under the device notch.
		var editor = new Editor
		{
			AutomationId = "TestEditor",
			Text = "This editor should be fully visible in landscape — not hidden under the notch",
			HeightRequest = 200,
			BackgroundColor = Colors.LightYellow,
		};

		Content = new ScrollView
		{
			AutomationId = "TestScrollView",
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Padding = new Thickness(0),
				Children =
				{
					new Label
					{
						AutomationId = "PageTitle",
						Text = "Issue 35410 — Landscape notch test",
						FontAttributes = FontAttributes.Bold,
					},
					editor,
					new Label
					{
						Text = "Content below editor — should scroll normally",
					},
				}
			}
		};
	}
}
