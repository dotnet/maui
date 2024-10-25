namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25193, "Background gradients don't work for some views", PlatformAffected.iOS)]
	public partial class Issue25193 : ContentPage
	{
		public Issue25193()
		{
			InitializeComponent();
		}
	}
}