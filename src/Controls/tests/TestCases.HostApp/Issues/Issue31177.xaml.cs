namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31177, "ScrollView.ScrollToAsync doesn't work when called from Page.OnAppearing", PlatformAffected.iOS | PlatformAffected.Android)]
public partial class Issue31177 : ContentPage
{
	public Issue31177()
	{
		InitializeComponent();
	}
	protected override async void OnAppearing()
	{
		await scrollView.ScrollToAsync(0, 2000, true);
	}
}