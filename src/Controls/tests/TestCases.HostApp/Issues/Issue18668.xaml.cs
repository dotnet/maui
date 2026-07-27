namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18668, "Visual state change for disabled RadioButton", PlatformAffected.All)]
	public partial class Issue18668 : ContentPage
	{
		public Issue18668()
		{
			InitializeComponent();
		}

		private void ButtonClicked(object sender, EventArgs e)
		{
			radioButton.IsEnabled = false;
		}
	}
}