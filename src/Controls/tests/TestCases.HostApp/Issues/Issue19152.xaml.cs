namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19152, "Windows | Entry ClearButton not taking color of text", PlatformAffected.UWP)]
	public partial class Issue19152 : ContentPage
	{
		public Issue19152()
		{
			InitializeComponent();
		}
		private void OnCounterClicked(object sender, EventArgs e)
		{
			entry.Focus();
		}
	}
}