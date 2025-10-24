#nullable enable
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30539, "Clicking on a target=\"_blank\" link in WebView on iOS does not do anything", PlatformAffected.iOS)]
public partial class Issue30539 : ContentPage
{
	public Issue30539()
	{
		InitializeComponent();
		
		webView.Navigating += WebView_Navigating;
		webView.Navigated += WebView_Navigated;
	}

	private void WebView_Navigating(object? sender, WebNavigatingEventArgs e)
	{
		var navigatingLabel = this.FindByName<Label>("NavigatingLabel");
		var urlLabel = this.FindByName<Label>("UrlLabel");
		
		if (navigatingLabel != null)
		{
			navigatingLabel.Text = "Navigating event triggered";
			navigatingLabel.TextColor = Colors.Green;
		}
		
		if (urlLabel != null)
		{
			urlLabel.Text = $"URL: {e.Url}";
		}
		
		// Cancel the navigation to prevent opening external browser
		// This tests that the developer can control the behavior
		e.Cancel = true;
		
		var cancelLabel = this.FindByName<Label>("CancelLabel");
		if (cancelLabel != null)
		{
			cancelLabel.Text = "Can cancel: Yes (navigation cancelled)";
			cancelLabel.TextColor = Colors.Green;
		}
	}

	private void WebView_Navigated(object? sender, WebNavigatedEventArgs e)
	{
		// This should not be called when we cancel the navigation
	}
}
