namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28303, "[Windows] WebView Navigated event called after cancelling it", PlatformAffected.UWP, isInternetRequired: true)]
public partial class Issue28303 : ContentPage
{
	public Issue28303()
	{
		var verticalStackLayout = new VerticalStackLayout();
		verticalStackLayout.Spacing = 20;
		var webView = new WebView()
		{
			HeightRequest = 300,
			WidthRequest = 400,
			AutomationId = "webView",
			Source = "https://learn.microsoft.com/dotnet/maui"
		};

		var label1 = new Label
		{
			Text = "WebView Navigating event is not triggered",
			AutomationId = "navigatingLabel"
		};

		var label = new Label
		{
			Text = "WebView Navigated event is not triggered",
			AutomationId = "navigatedLabel"
		};

		webView.Navigating += (s, e) =>
		{
			label1.Text = "Navigating event is triggered";
			e.Cancel = true;
		};

		webView.Navigated += (s, e) =>
		{
			label.Text = "Navigated event is triggered";
		};

		verticalStackLayout.Add(label1);
		verticalStackLayout.Add(label);
		verticalStackLayout.Add(webView);

		Content = verticalStackLayout;
	}
}