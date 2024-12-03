namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25975, "Double tapping Editor control locks app", PlatformAffected.All)]
	public class Issue25975 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor()
			{
				Text = "Hello",
				AutomationId = "DoubleTapEditor",
				FontSize = 24,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Fill,
				AutoSize = EditorAutoSizeOption.TextChanges,
			};

			var rec = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
			rec.Tapped += (s, e) => { editor.Text = "World"; };
			editor.GestureRecognizers.Add(rec);

			Content = editor;

		}
	}
}
