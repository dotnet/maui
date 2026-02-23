using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 3275, "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE", PlatformAffected.iOS)]
	public class Issue3275 : ContentPage
	{
		const string RunTestId = "RunTest";
		const string TestResultId = "TestResult";

		Label _statusLabel;

		public Issue3275()
		{
			_statusLabel = new Label
			{
				Text = "Ready",
				AutomationId = TestResultId,
				FontSize = 20,
			};

			var runButton = new Button
			{
				Text = "Run Test",
				AutomationId = RunTestId,
			};

			runButton.Clicked += OnRunTest;

			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 20,
				Children = { runButton, _statusLabel }
			};
		}

		async void OnRunTest(object sender, EventArgs e)
		{
			_statusLabel.Text = "Running...";
			try
			{
				// Build a ListView using RecycleElement caching strategy
				var viewModel = new TransactionsViewModel();
				FastListView listView = null;
				listView = new FastListView
				{
					HasUnevenRows = true,
					ItemTemplate = new DataTemplate(() =>
					{
						var viewCell = new ViewCell();
						var item = new MenuItem { Text = "test" };
						item.SetBinding(MenuItem.CommandProperty, new Binding("BindingContext.RepeatCommand", source: listView));
						item.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
						viewCell.ContextActions.Add(item);
						var lbl = new Label();
						lbl.SetBinding(Label.TextProperty, "Name");
						viewCell.View = lbl;
						return viewCell;
					})
				};
				listView.SetBinding(ListView.ItemsSourceProperty, "Items");
				listView.BindingContext = viewModel;

				// Show the ListView
				var container = (VerticalStackLayout)Content;
				container.Children.Add(listView);

				// Wait for ListView to render
				await Task.Delay(1000);

				// Perform ScrollTo — this is the operation that causes cell leak/NRE in the bug
				var targetItem = viewModel.Items.Skip(25).First();
				listView.ScrollTo(targetItem, ScrollToPosition.MakeVisible, false);

				await Task.Delay(500);

				// Simulate what Prism.Forms does: set BindingContext to null
				// This triggers the leak/NRE when combined with RecycleElement + ScrollTo
				listView.BindingContext = null;

				await Task.Delay(500);

				// Remove the ListView — simulates navigating away
				container.Children.Remove(listView);

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