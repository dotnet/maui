using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Controls
{
	public class ListRefresh : ContentPage
	{
		public ListRefresh ()
		{
			var refreshingCount = 0;

			var grid = new Grid ();
			var fooViewModel = new FooViewModel ();
			var lv = new ListView {BindingContext = fooViewModel, IsGroupingEnabled = true, GroupDisplayBinding = new Binding ("Name"), IsPullToRefreshEnabled = false};

			var stack = new StackLayout ();
			var btn = new Button { Text = string.Format ("IsRefreshing {0}", lv.IsRefreshing) };
			btn.Command = new Command (s => {
				lv.IsRefreshing = !lv.IsRefreshing;
				btn.Text = string.Format("IsRefreshing {0}",lv.IsRefreshing);
			});

			var btn4 = new Button { Text = "BeginRefresh", Command = new Command (s => {
				lv.BeginRefresh();
				btn.Text = string.Format("IsRefreshing {0}",lv.IsRefreshing);
			}) };
			var btn1 = new Button { Text = "EndRefresh", Command = new Command (s => {
				lv.EndRefresh();
				btn.Text = string.Format("IsRefreshing {0}",lv.IsRefreshing);
			}) };

			var btn2 = new Button { Text = string.Format ("Pull {0}", lv.IsPullToRefreshEnabled) };
			btn2.Command = new Command (s => {
				lv.IsPullToRefreshEnabled = !lv.IsPullToRefreshEnabled;
				btn2.Text = string.Format("Pull {0}",lv.IsPullToRefreshEnabled);
			});

			var btn3 = new Button { Text = string.Format("CanExecute {0}",fooViewModel.CanExecute) };
			btn3.Command = new Command (s => {
				fooViewModel.CanExecute = !fooViewModel.CanExecute;
				btn3.Text = string.Format("CanExecute {0}",fooViewModel.CanExecute);
			});
		
			var lbl = new Label { Text = string.Format ("Refreshing {0}", refreshingCount) };
			lv.Refreshing += (object sender, EventArgs e) => {
				refreshingCount++;
				lbl.Text = string.Format ("Refreshing {0}", refreshingCount);
			};

			stack.Children.Add (btn);
			stack.Children.Add (btn4);
			stack.Children.Add (btn1);
			stack.Children.Add (btn2);
			stack.Children.Add (btn3);
			stack.Children.Add (lbl);
			lv.Header =  new ContentView { HeightRequest = 300, HorizontalOptions = LayoutOptions.FillAndExpand, Content = stack };

			lv.SetBinding (ListView.ItemsSourceProperty, "Things");
			lv.SetBinding (ListView.RefreshCommandProperty, "RefreshThingsCommand");
			grid.Children.Add (lv, 0, 0);

			Content = grid;
		}

		public class FooViewModel
		{
			List<Group<string>> _things;
			public List<Group<string>> Things {
				get 
				{ 
					return _things ?? (_things = new List<Group<string>> {
						new Group<string>(new []{"A","B","C","D","E","F","G","H","I","J","K"}) {Name = "Letters"},
						new Group<string>(new []{"1","2","3","4","5","6","7","8","9","10"}) {Name = "Numbers"}
					}); 
				}
			}

			bool _canExecute;
			public bool CanExecute {
				get 
				{ 
					return _canExecute;
				}
				set { 
					_canExecute = value;
					RefreshThingsCommand.ChangeCanExecute ();
				}
			}

			Command _refreshThingsCommand;
			public Command RefreshThingsCommand {
				get {return _refreshThingsCommand ?? (_refreshThingsCommand = new Command (BeginRefreshThings, () => _canExecute ));}
			}

			protected void BeginRefreshThings()
			{

			}
		}

		public class Group<T> : ObservableCollection<T>
		{
			public Group (IEnumerable<T> seed) : base(seed){}

			public string Name {
				get;
				set;
			}
		}
	}
}


