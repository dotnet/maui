using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
[Issue(IssueTracker.Github, 3275, "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE", PlatformAffected.iOS)]
public class Issue3275 : ContentPage
{
public Issue3275()
{
var statusLabel = new Label
{
Text = "Ready",
AutomationId = "TestResult",
FontSize = 20,
};

var items = new ObservableCollection<string>(
Enumerable.Range(1, 50).Select(i => $"Item {i}"));

var listView = new ListView(ListViewCachingStrategy.RecycleElement)
{
HasUnevenRows = true,
ItemsSource = items,
ItemTemplate = new DataTemplate(() =>
{
var cell = new ViewCell();
var lbl = new Label();
lbl.SetBinding(Label.TextProperty, ".");
cell.View = lbl;
return cell;
}),
HeightRequest = 300,
};

var runButton = new Button
{
Text = "Run Test",
AutomationId = "RunTest",
};

runButton.Clicked += (s, e) =>
{
statusLabel.Text = "Running...";
try
{
// ScrollTo an item in RecycleElement mode - this was causing cell leak/NRE
var target = items.Skip(25).First();
listView.ScrollTo(target, ScrollToPosition.MakeVisible, false);

// Null BindingContext like Prism.Forms does - this triggered the NRE
listView.BindingContext = null;

// If we get here without NRE, the bug is fixed
statusLabel.Text = "SUCCESS";
}
catch (Exception ex)
{
statusLabel.Text = $"FAIL: {ex.Message}";
}
};

Content = new VerticalStackLayout
{
Spacing = 10,
Padding = 20,
Children = { runButton, statusLabel, listView }
};
}
}
}
