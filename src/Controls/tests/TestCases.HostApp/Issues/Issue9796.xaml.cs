namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 9796, "[Android]Editor controls don't raise Completed event consistently",
		PlatformAffected.Android)]
	public partial class Issue9796 : ContentPage
	{
		public Issue9796()
		{
			InitializeComponent();
		}

		private void OnEditorCompleted(object sender, EventArgs e)
		{
			Label.Text = "Triggered";	
		}

		private void OnFocusButtonClicked(object sender, EventArgs e)
		{
			Editor.Focus();
		}

		private void UnFocusButtonClicked(object sender, EventArgs e)
		{
			Editor.Unfocus();
		}
	}
}
