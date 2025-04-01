namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21846, "Fix crash closing Popup with WebView", PlatformAffected.iOS, isInternetRequired: true)]
	public partial class Issue21846 : ContentPage
	{
		public Issue21846()
		{
			InitializeComponent();
		}

		async void OnButtonClicked(object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new Issue21846Modal());
		}
	}
}
