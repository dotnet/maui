namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 15991, "FlexLayout Padding not working", PlatformAffected.iOS)]
	public partial class Issue15991 : ContentPage
	{
		public Issue15991()
		{
			InitializeComponent();
		}
	}
}