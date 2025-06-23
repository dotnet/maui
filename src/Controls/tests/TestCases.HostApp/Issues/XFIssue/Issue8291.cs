namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8291, "[Android] Editor - Text selection menu does not appear when selecting text on an editor placed within a ScrollView", PlatformAffected.Android)]
public class Issue8291 : TestContentPage
{
	protected override void Init()
	{
		Content = new StackLayout()
		{
			new Label()
			{
				Text = "Only Relevant on Android"
			},
			new ScrollView()
			{
				Content = new Editor()
				{
					Text = "Press and hold this text. Text should become selected and context menu should open",
					AutomationId = "PressEditor"
				}
			},
			new ScrollView()
			{
				Content = new Entry()
				{
					Text = "Press and hold this text. Text should become selected and context menu should open",
					AutomationId = "PressEntry"
				}
			}
		};
	}
}
