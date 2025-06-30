using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29233, "Android WebView Navigated is fired without setting source", PlatformAffected.Android)]
public partial class Issue29233 : ContentPage
{
	public Issue29233()
	{
		InitializeComponent();
	}

	private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
	{
		label.IsVisible = true;
		label.Text = "Failed";
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(2000);
		waitLabel.Text = "Hello";
	}
}