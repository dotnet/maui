using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using ListView = Microsoft.Maui.Controls.ListView;

namespace Maui.Controls.Sample.Issues
{
[Issue(IssueTracker.Github, 1905, "Pull to refresh doesn't work if iOS 11 large titles is enabled", PlatformAffected.iOS)]
public class Issue1905LargeTitles : TestNavigationPage
{
protected override void Init()
{
// Large titles is the specific trigger for this bug
On<iOS>().SetPrefersLargeTitles(true);

var statusLabel = new Label
{
Text = "Ready",
AutomationId = "TestResult",
FontSize = 20,
};

bool refreshCompleted = false;

var items = new List<string>();
for (int i = 0; i < 20; i++)
{
items.Add($"pull to {i}");
}

var lst = new ListView
{
IsPullToRefreshEnabled = true,
ItemsSource = items,
};

lst.RefreshCommand = new Command(async () =>
{
await Task.Delay(500);
lst.ItemsSource = new List<string> { "data refreshed" };
lst.EndRefresh();
refreshCompleted = true;
});

var runButton = new Button
{
Text = "Run Test",
AutomationId = "RunTest",
};

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

var page = new ContentPage
{
Title = "Pull Large Titles",
Content = new VerticalStackLayout
{
Children = { runButton, statusLabel, lst }
}
};

Navigation.PushAsync(page);
}
}
}
