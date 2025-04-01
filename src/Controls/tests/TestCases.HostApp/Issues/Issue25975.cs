namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25975, "Double tapping Editor control locks app", PlatformAffected.All)]
	public class Issue25975 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor()
			{
				Text = "MAUI",
				AutomationId = "DoubleTapEditor",
				FontSize = 24,
				MaxLength = 5,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Fill,
				AutoSize = EditorAutoSizeOption.TextChanges,
			};

			var stackLayout = new StackLayout()
			{
				Children =
				{
					editor
				}
			};

			Content = stackLayout;

		}
	}
}
