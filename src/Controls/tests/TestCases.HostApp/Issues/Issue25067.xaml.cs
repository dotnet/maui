namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25067, "[Android] SearchHandler default/custom icons", PlatformAffected.Android)]
	public partial class Issue25067 : Shell
	{
		public Issue25067()
		{
			InitializeComponent();
		}
		private void GoToCustom_Clicked(object sender, EventArgs e)
		{
			GoToAsync("//CustomIcons");
		}
		private void GoToDefault_Clicked(object sender, EventArgs e)
		{
			GoToAsync("//DefaultIcons");
		}
	}
}
