namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25975, "Double tapping Editor control locks app", PlatformAffected.All)]
	public class Issue25975 : TestContentPage
	{
		protected override void Init()
		{
			Content =
				new VerticalStackLayout()
				{
					new Editor()
					{
						Text = "Hello",
						AutomationId = "Editor",
						FontSize = 24,
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalOptions= LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Fill,
						AutoSize = EditorAutoSizeOption.TextChanges,
					},
				};
		}
	}
}
