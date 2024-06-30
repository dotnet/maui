namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20264, "Android does not update visibility of a visual element if it has a Shadow", PlatformAffected.Android)]
	public partial class Issue20264 : ContentPage
	{
		public Issue20264()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			label.IsVisible = true;
		}
	}
}