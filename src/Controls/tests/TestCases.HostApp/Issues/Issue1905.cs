using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using ListView = Microsoft.Maui.Controls.ListView;

namespace Maui.Controls.Sample.Issues
{
[Issue(IssueTracker.Github, 1905, "Pull to refresh doesn't work if iOS 11 large titles is enabled", PlatformAffected.iOS)]
public class Issue1905LargeTitles : ContentPage
{
public Issue1905LargeTitles()
{
var statusLabel = new Label
{
Text = "Ready",
AutomationId = "TestResult",
FontSize = 20,
};

var runButton = new Button
{
Text = "Run Test",
AutomationId = "RunTest",
};

bool refreshCompleted = false;

var lst = new ListView
{
IsPullToRefreshEnabled = true,
ItemsSource = new List<string> { "initial item" },
HeightRequest = 200,
};

lst.RefreshCommand = new Command(async () =>
{
await Task.Delay(500);
lst.ItemsSource = new List<string> { "data refreshed" };
lst.EndRefresh();
refreshCompleted = true;
});

runButton.Clicked += async (s, e) =>
{
statusLabel.Text = "Running...";
refreshCompleted = false;

// BeginRefresh programmatically — this is what fails with large titles
lst.BeginRefresh();

// Wait for refresh to complete (max 10s)
for (int i = 0; i < 20; i++)
{
await Task.Delay(500);
if (refreshCompleted)
break;
}

statusLabel.Text = refreshCompleted ? "SUCCESS" : "FAIL: refresh did not complete";
};

Content = new VerticalStackLayout
{
Spacing = 10,
Padding = 20,
Children = { runButton, statusLabel, lst }
};
}
}
}
