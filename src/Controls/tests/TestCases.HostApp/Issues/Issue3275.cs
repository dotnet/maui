using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3275, "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE", PlatformAffected.iOS)]
	public class Issue3275 : NavigationPage
	{
		public Issue3275() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			static readonly string BtnLeakId = "btnLeak";
			static readonly string BtnScrollToId = "btnScrollTo";

			public MainPage()
			{
				var layout = new StackLayout();

				var btn = new Button
				{
					Text = " Leak 1 ",
					AutomationId = BtnLeakId,
					Command = new Command(() =>
					{
						Navigation.PushAsync(new Issue3275TransactionsPage1());
					})
				};
				layout.Children.Add(btn);
				Content = layout;
			}



			public class Issue3275TransactionsPage1 : ContentPage
			{
				private readonly TransactionsViewModel _viewModel = new TransactionsViewModel();
				FastListView _transactionsListView;

				public Issue3275TransactionsPage1()
				{
					var grd = new Grid();
					grd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					grd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					grd.RowDefinitions.Add(new RowDefinition());
					_transactionsListView = new FastListView
					{
						HasUnevenRows = true,
						ItemTemplate = new DataTemplate(() =>
						{
							var viewCell = new ViewCell();
							var item = new MenuItem
							{
								Text = "test"
							};
							item.SetBinding(MenuItem.CommandProperty, new Binding("BindingContext.RepeatCommand", source: _transactionsListView));
							item.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
							viewCell.ContextActions.Add(item);
							var lbl = new Label();
							lbl.SetBinding(Label.TextProperty, "Name");
							viewCell.View = lbl;
							return viewCell;
						})
					};
					_transactionsListView.SetBinding(ListView.ItemsSourceProperty, "Items");

					grd.Children.Add(new Label
					{
						Text = "Click 'Scroll To' and go back"
					});

					var btn = new Button
					{
						Text = "Scroll to",
						AutomationId = BtnScrollToId,
						Command = new Command(() =>
						{
							var item = _viewModel.Items.Skip(250).First();
							_transactionsListView.ScrollTo(item, ScrollToPosition.MakeVisible, false);
						})
					};

					Grid.SetRow(btn, 1);
					grd.Children.Add(btn);
					Grid.SetRow(_transactionsListView, 2);
					grd.Children.Add(_transactionsListView);

					Content = grd;


					BindingContext = _viewModel;
				}

				protected override void OnDisappearing()
				{
					BindingContext = null; // IMPORTANT!!! Prism.Forms does this under the hood
				}
			}

			public sealed class FastListView : ListView
			{
				public FastListView() : base(ListViewCachingStrategy.RecycleElement)
				{
				}
			}

			public class TransactionsViewModel
			{
				public TransactionsViewModel()
				{
					var items = Enumerable.Range(1, 500).Select(i => new Item { Name = i.ToString() });

					Items = new ObservableCollection<Item>(items);

					RepeatCommand = new AsyncDelegateCommand<object>(Repeat, x => true);
				}

				public ObservableCollection<Item> Items { get; }

				public AsyncDelegateCommand<object> RepeatCommand { get; }

				private Task Repeat(object item)
				{
					return Task.CompletedTask;
				}
			}

			public class Item
			{
				public string Name { get; set; }
			}

			public sealed class AsyncDelegateCommand<T> : ICommand
			{
#pragma warning disable 0067
				public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

				private readonly Func<T, Task> _executeMethod;
				private readonly Func<T, bool> _canExecuteMethod;
				private bool _isInFlight;

#if XAMARIN
		private DateTime _lastExecuted = DateTime.MinValue;
#endif

				public AsyncDelegateCommand(Func<T, Task> executeMethod)
					: this(executeMethod, _ => true)
				{
				}

				public AsyncDelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
				{
					var genericType = typeof(T);

					// DelegateCommand allows object or Nullable<>.  
					// note: Nullable<> is a struct so we cannot use a class constraint.
					if (genericType.IsValueType)
					{
						if (!genericType.IsGenericType || !typeof(Nullable<>).IsAssignableFrom(genericType.GetGenericTypeDefinition()))
							throw new InvalidCastException("T for DelegateCommand<T> is not an object nor Nullable.");
					}

					_executeMethod = executeMethod;
					_canExecuteMethod = canExecuteMethod;
				}

				internal async Task Execute(T parameter)
				{
					if (_isInFlight)
						return;

#if XAMARIN
			if (DateTime.UtcNow.Subtract(_lastExecuted).TotalMilliseconds < 200d)
				return;
#endif

					try
					{
						_isInFlight = true;

						await _executeMethod(parameter);
					}
					finally
					{
						_isInFlight = false;

#if XAMARIN
				_lastExecuted = DateTime.UtcNow;
#endif
					}
				}

				internal bool CanExecute(T parameter)
				{
					return _canExecuteMethod(parameter);
				}

				public void Execute(object parameter)
				{
					//Execute((T)parameter).NotWait();
				}

				public bool CanExecute(object parameter)
				{
					return CanExecute((T)parameter);
				}
			}
		}
	}
}