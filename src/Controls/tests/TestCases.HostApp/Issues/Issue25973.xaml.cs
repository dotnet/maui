namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25973, "Editor vertical text alignment not working after toggling IsVisible", PlatformAffected.UWP)]
	public partial class Issue25973 : ContentPage
	{
		public Issue25973()
		{
			InitializeComponent();
		}

		private void VisibilityButtonClicked(object sender, EventArgs e)
		{
			editor.IsVisible = true;
		}
	}
}