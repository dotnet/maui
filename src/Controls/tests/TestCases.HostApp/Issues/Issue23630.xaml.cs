namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, "23630", "Shadow not visible in Button When using Clipping", PlatformAffected.iOS)]
	public partial class Issue23630 : ContentPage
	{
		public Issue23630()
		{
			InitializeComponent();
		}
	}	
}