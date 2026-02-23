using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
[Issue(IssueTracker.Github, 3275, "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE", PlatformAffected.iOS)]
public class Issue3275 : ContentPage
{
Label _statusLabel;

public Issue3275()
{
_statusLabel = new Label
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

runButton.Clicked += OnRunTestClicked;

Content = new VerticalStackLayout
{
Spacing = 10,
Padding = 20,
Children = { runButton, _statusLabel }
};
}

async void OnRunTestClicked(object sender, EventArgs e)
{
_statusLabel.Text = "Running...";
try
{
// Push a ListView page with RecycleElement, ContextActions, and command bindings
var transactionsPage = new TransactionsPage();
await Navigation.PushModalAsync(transactionsPage);

// Wait for page to load
await Task.Delay(500);

// Perform ScrollTo — this causes cell recycling and is part of the leak trigger
transactionsPage.DoScrollTo();

await Task.Delay(500);

// Pop the page — triggers OnDisappearing which nulls BindingContext,
// causing NRE on recycled cells with ContextAction command bindings
await Navigation.PopModalAsync();

await Task.Delay(500);

_statusLabel.Text = "SUCCESS";
}
catch (Exception ex)
{
_statusLabel.Text = $"FAIL: {ex.Message}";
}
}

class TransactionsPage : ContentPage
{
readonly TransactionsViewModel _viewModel = new();
readonly FastListView _listView;

public TransactionsPage()
{
Title = "Transactions";
_listView = new FastListView
{
HasUnevenRows = true,
ItemTemplate = new DataTemplate(() =>
{
var viewCell = new ViewCell();
// ContextActions with command bindings back to ListView's BindingContext
// are part of the original leak/NRE surface area
var item = new MenuItem { Text = "test" };
item.SetBinding(MenuItem.CommandProperty, new Binding("BindingContext.RepeatCommand", source: _listView));
item.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
viewCell.ContextActions.Add(item);
var lbl = new Label();
lbl.SetBinding(Label.TextProperty, "Name");
viewCell.View = lbl;
return viewCell;
})
};
_listView.SetBinding(ListView.ItemsSourceProperty, "Items");
Content = _listView;
BindingContext = _viewModel;
}

public void DoScrollTo()
{
var item = _viewModel.Items.Skip(25).First();
_listView.ScrollTo(item, ScrollToPosition.MakeVisible, false);
}

protected override void OnDisappearing()
{
BindingContext = null; // IMPORTANT!!! Prism.Forms does this under the hood
}
}

sealed class FastListView : ListView
{
public FastListView() : base(ListViewCachingStrategy.RecycleElement) { }
}

class TransactionsViewModel
{
public TransactionsViewModel()
{
Items = new ObservableCollection<Item>(
Enumerable.Range(1, 50).Select(i => new Item { Name = i.ToString() }));
RepeatCommand = new Command(_ => { });
}

public ObservableCollection<Item> Items { get; }
public ICommand RepeatCommand { get; }
}

class Item
{
public string Name { get; set; }
}
}
}
