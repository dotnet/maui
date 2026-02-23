namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31238, "Setting CharacterSpacing property on buttons causing crashing on iOS", PlatformAffected.iOS)]
public class Issue31238 : TestContentPage
{
	protected override void Init()
	{
		var btn = new Button
		{
			Text = "Click me",
			AutomationId = "MauiButton",
			CharacterSpacing = 5,
			TextColor = Colors.Red
		};

		int clicks = 0;
		btn.Command = new Command(() =>
		{
			clicks = (clicks + 1) % 3;
			btn.CharacterSpacing = clicks == 0 ? 0 : clicks;
		});

		Content = btn;
	}
}