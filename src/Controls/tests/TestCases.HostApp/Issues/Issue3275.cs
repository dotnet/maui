using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 3275, "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE", PlatformAffected.iOS)]
	public class Issue3275 : NavigationPage
	{
		public Issue3275() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			static readonly string BtnLeakId = "btnLeak";
			Label _statusLabel;

			public MainPage()
			{
				_statusLabel = new Label
				{
					Text = "Ready",
					AutomationId = "TestResult",
					FontSize = 20,
				};

				var btn = new Button
				{
					Text = " Leak 1 ",
					AutomationId = BtnLeakId,
					Command = new Command(() =>
					{
						_statusLabel.Text = "Navigated";
						Navigation.PushAsync(new TransactionsPage());
					})
				};

				Content = new VerticalStackLayout
				{
					Spacing = 10,
					Padding = 20,
					Children = { btn, _statusLabel }
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				// If we returned from the TransactionsPage without crashing, the test passed.
				// The original bug causes an NRE crash during back navigation when
				// OnDisappearing nulls BindingContext on recycled cells with ContextActions.
				if (_statusLabel.Text == "Navigated")
				{
					_statusLabel.Text = "SUCCESS";
				}
			}
		}

		public class TransactionsPage : ContentPage
		{
			static readonly string BtnScrollToId = "btnScrollTo";
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

				var btn = new Button
				{
					Text = "Scroll to",
					AutomationId = BtnScrollToId,
					Command = new Command(() =>
					{
						var item = _viewModel.Items.Skip(25).First();
						_listView.ScrollTo(item, ScrollToPosition.MakeVisible, false);
					})
				};

				var grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				grid.RowDefinitions.Add(new RowDefinition());
				grid.Children.Add(btn);
				Grid.SetRow(_listView, 1);
				grid.Children.Add(_listView);
				Content = grid;
				BindingContext = _viewModel;
			}

			protected override void OnDisappearing()
			{
				BindingContext = null; // IMPORTANT!!! Prism.Forms does this under the hood
			}
		}

		public sealed class FastListView : ListView
		{
			public FastListView() : base(ListViewCachingStrategy.RecycleElement) { }
		}

		public class TransactionsViewModel
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

		public class Item
		{
			public string Name { get; set; }
		}
	}
}
