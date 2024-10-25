namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 10987, "Editor HorizontalTextAlignment Does not Works.", PlatformAffected.All)]
	public partial class Issue10987 : ContentPage
	{
		public Issue10987()
		{
			InitializeComponent();
		}

		private void UpdateButtonClicked(object sender, EventArgs e)
		{
			editor.VerticalTextAlignment = TextAlignment.End;
			editor.HorizontalTextAlignment = TextAlignment.End;

			editor1.VerticalTextAlignment = TextAlignment.End;
			editor1.HorizontalTextAlignment = TextAlignment.End;
		}

		private void ResetButtonClicked(object sender, EventArgs e)
		{
			editor.VerticalTextAlignment = TextAlignment.Start;
			editor.HorizontalTextAlignment = TextAlignment.Start;
			editor.Unfocus();

			editor1.VerticalTextAlignment = TextAlignment.Start;
			editor1.HorizontalTextAlignment = TextAlignment.Start;
			editor1.Unfocus();
		}
	}
}