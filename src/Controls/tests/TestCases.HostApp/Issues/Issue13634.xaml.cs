namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 13634, "Scrolling of Editor placed in ScollView does not work", PlatformAffected.Android)]
	public partial class Issue13634 : ContentPage
	{
		public Issue13634()
		{
			InitializeComponent();
		}
	}
}