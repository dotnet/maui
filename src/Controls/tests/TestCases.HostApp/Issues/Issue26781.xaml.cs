namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26781, "ScrollView.ScrollToAsync doesn't work when called from Page.OnAppearing", PlatformAffected.iOS | PlatformAffected.Android)]
	public partial class Issue26781 : ContentPage
	{
		public Issue26781()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			await scrollView.ScrollToAsync(0, 2000, true);
		}
	}
}
