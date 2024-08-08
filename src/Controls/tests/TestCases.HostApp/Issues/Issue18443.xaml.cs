namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18443, "SelectionLength Property Not Applied to Entry at Runtime", PlatformAffected.Android)]
	public partial class Issue18443 : ContentPage
	{
		public Issue18443()
		{
			InitializeComponent();
			entry.Focused += (sender, e) =>
			{
				entry.SelectionLength = 3;
			};
		}
	}
}