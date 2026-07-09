using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 35470, "WebView AllowedDomains blocks navigation to non-allowed domains", PlatformAffected.All)]
public class Issue35470 : ContentPage
{
	public Issue35470()
	{
		var statusLabel = new Label
		{
			Text = "Waiting...",
			AutomationId = "StatusLabel"
		};

		var navigatingLabel = new Label
		{
			Text = "",
			AutomationId = "NavigatingLabel"
		};

		var navigatedLabel = new Label
		{
			Text = "",
			AutomationId = "NavigatedLabel"
		};

		var blockedLabel = new Label
		{
			Text = "",
			AutomationId = "BlockedLabel"
		};

		var webView = new WebView
		{
			HeightRequest = 400,
			WidthRequest = 400,
			AutomationId = "TestWebView",
			AllowedDomains = new List<string> { "example.com" },
		};

		webView.Navigating += (s, e) =>
		{
			navigatingLabel.Text = $"Navigating: {e.Url}";
		};

		webView.Navigated += (s, e) =>
		{
			navigatedLabel.Text = $"Navigated: {e.Result} - {e.Url}";
			statusLabel.Text = "NavigationComplete";
		};

		var loadAllowedButton = new Button
		{
			Text = "Load Allowed (example.com)",
			AutomationId = "LoadAllowedButton",
		};
		loadAllowedButton.Clicked += (s, e) =>
		{
			statusLabel.Text = "Loading...";
			blockedLabel.Text = "";
			webView.Source = new UrlWebViewSource { Url = "https://example.com" };
		};

		var loadBlockedButton = new Button
		{
			Text = "Load Blocked (evil.com)",
			AutomationId = "LoadBlockedButton",
		};
		loadBlockedButton.Clicked += (s, e) =>
		{
			statusLabel.Text = "Loading...";
			webView.Source = new UrlWebViewSource { Url = "https://evil.com" };
			// Since the navigation is blocked, we won't get a Navigated event
			// Set a timer to detect if navigation was blocked
			Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(3), () =>
			{
				if (statusLabel.Text == "Loading...")
				{
					statusLabel.Text = "Blocked";
					blockedLabel.Text = "NavigationBlocked";
				}
			});
		};

		var loadNoRestrictionsButton = new Button
		{
			Text = "Remove AllowedDomains",
			AutomationId = "RemoveAllowedDomainsButton",
		};
		loadNoRestrictionsButton.Clicked += (s, e) =>
		{
			webView.AllowedDomains = null;
			statusLabel.Text = "AllowedDomainsRemoved";
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 5,
				Padding = 10,
				Children =
				{
					statusLabel,
					navigatingLabel,
					navigatedLabel,
					blockedLabel,
					loadAllowedButton,
					loadBlockedButton,
					loadNoRestrictionsButton,
					webView,
				}
			}
		};
	}
}
