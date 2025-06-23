namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, "25201", "[Android] ImageButton Padding Incorrect After IsVisible False", PlatformAffected.Android)]
	public partial class Issue25201 : ContentPage
	{
		public Issue25201()
		{
			InitializeComponent();
		}
	}
}