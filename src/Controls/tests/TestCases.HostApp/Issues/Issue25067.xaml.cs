namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25067, "Render default icons in SearchHandler", PlatformAffected.Android)]
	public partial class Issue25067 : Shell
	{
		public Issue25067()
		{
			InitializeComponent();
		}
	}
}
