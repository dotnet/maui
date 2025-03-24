namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.ManualTest, "C3", "Can scroll ListView inside RefreshView", PlatformAffected.All, isInternetRequired: true)]
	public partial class Issue18896 : ContentPage
	{
		public Issue18896()
		{
			InitializeComponent();

			BindingContext = new MonkeysViewModel();
		}
	}
}