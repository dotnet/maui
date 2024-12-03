namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25975, "Double tapping Editor control locks app", PlatformAffected.All)]
	public class Issue25975 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor()
			{
				Text = "Editor",
				AutomationId = "DoubleTapEditor",
				FontSize = 24,
				MaxLength = 5,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Fill,
				AutoSize = EditorAutoSizeOption.TextChanges,
			};

			var entry = new Entry()
			{
				Text = "Entry",
				AutomationId = "DoubleTapEntry",
				FontSize = 24,
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Fill,
			};

			var stackLayout = new StackLayout()
			{
				Children =
				{
					editor, entry
				}
			};

			Content = stackLayout;

		}
	}
}
