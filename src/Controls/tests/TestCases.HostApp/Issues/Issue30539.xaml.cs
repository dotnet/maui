#nullable enable
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30539, "Clicking on a target=\"_blank\" link in WebView on iOS does not do anything", PlatformAffected.iOS)]
public partial class Issue30539 : ContentPage
{
	public Issue30539()
	{
		InitializeComponent();

		webView.Navigating += WebView_Navigating;
	}

	private async void OnTriggerLinkClicked(object? sender, EventArgs e)
	{
		// Use JavaScript to programmatically click the target="_blank" link
		await webView.EvaluateJavaScriptAsync("document.getElementById('testLink').click();");
	}

	private void WebView_Navigating(object? sender, WebNavigatingEventArgs e)
	{
		navigatingLabel.Text = "Navigating event triggered";
		navigatingLabel.TextColor = Colors.Green;

		urlLabel.Text = $"URL: {e.Url}";

		// Cancel the navigation to prevent opening external browser
		// This tests that the developer can control the behavior
		e.Cancel = true;

		cancelLabel.Text = "Can cancel: Yes (navigation cancelled)";
		cancelLabel.TextColor = Colors.Green;
	}
}
