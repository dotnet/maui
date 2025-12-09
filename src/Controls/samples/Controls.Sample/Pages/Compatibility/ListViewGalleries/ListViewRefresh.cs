using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ListViewGalleries
{
	public class ListViewRefresh : ContentPage
	{
		[RequiresUnreferencedCode("ListViewRefresh may require unreferenced code for data binding")]
		public ListViewRefresh()
		{
			var refreshingCount = 0;

			var grid = new Grid();
			var fooViewModel = new FooViewModel();
			var lv = new ListView { BindingContext = fooViewModel, IsGroupingEnabled = true, GroupDisplayBinding = new Binding("Name"), IsPullToRefreshEnabled = false };

			var headerGrid = new Grid();
			headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			headerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
			headerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
			headerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
			headerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
			headerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
			headerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

			var btn = new Button { Text = string.Format("IsRefreshing {0}", lv.IsRefreshing) };
			btn.Command = new Command(s =>
			{
				lv.IsRefreshing = !lv.IsRefreshing;
				btn.Text = string.Format("IsRefreshing {0}", lv.IsRefreshing);
			});

			var btn4 = new Button
			{
				Text = "BeginRefresh",
				Command = new Command(s =>
				{
					lv.BeginRefresh();
					btn.Text = string.Format("IsRefreshing {0}", lv.IsRefreshing);
				})
			};
			var btn1 = new Button
			{
				Text = "EndRefresh",
				Command = new Command(s =>
				{
					lv.EndRefresh();
					btn.Text = string.Format("IsRefreshing {0}", lv.IsRefreshing);
				})
			};

			var btn2 = new Button { Text = string.Format("Pull {0}", lv.IsPullToRefreshEnabled) };
			btn2.Command = new Command(s =>
			{
				lv.IsPullToRefreshEnabled = !lv.IsPullToRefreshEnabled;
				btn2.Text = string.Format("Pull {0}", lv.IsPullToRefreshEnabled);
			});

			var btn3 = new Button { Text = string.Format("CanExecute {0}", fooViewModel.CanExecute) };
			btn3.Command = new Command(s =>
			{
				fooViewModel.CanExecute = !fooViewModel.CanExecute;
				btn3.Text = string.Format("CanExecute {0}", fooViewModel.CanExecute);
			});

			var lbl = new Label { Text = string.Format("Refreshing {0}", refreshingCount) };
			lv.Refreshing += (object? sender, EventArgs e) =>
			{
				refreshingCount++;
				lbl.Text = string.Format("Refreshing {0}", refreshingCount);
			};

			headerGrid.Add(btn, 0, 0);
			headerGrid.Add(btn4, 0, 1);
			headerGrid.Add(btn1, 0, 2);
			headerGrid.Add(btn2, 0, 3);
			headerGrid.Add(btn3, 0, 4);
			headerGrid.Add(lbl, 0, 5);
			lv.Header = headerGrid;

			lv.SetBinding(ListView.ItemsSourceProperty, "Things");
			lv.SetBinding(ListView.RefreshCommandProperty, "RefreshThingsCommand");
			grid.Add(lv, 0, 6);

			Content = grid;
		}

		public class FooViewModel
		{
			List<Group<string>>? _things;
			public List<Group<string>> Things
			{
				get
				{
					return _things ?? (_things = new List<Group<string>> {
						new Group<string>(new []{"A","B","C","D","E","F","G","H","I","J","K"}) {Name = "Letters"},
						new Group<string>(new []{"1","2","3","4","5","6","7","8","9","10"}) {Name = "Numbers"}
					});
				}
			}

			bool _canExecute;
			public bool CanExecute
			{
				get
				{
					return _canExecute;
				}
				set
				{
					_canExecute = value;
					RefreshThingsCommand.ChangeCanExecute();
				}
			}

			Command? _refreshThingsCommand;
			public Command RefreshThingsCommand
			{
				get { return _refreshThingsCommand ?? (_refreshThingsCommand = new Command(BeginRefreshThings, () => _canExecute)); }
			}

			protected void BeginRefreshThings()
			{

			}
		}

		public class Group<T> : ObservableCollection<T>
		{
			public Group(IEnumerable<T> seed) : base(seed) { }

			public string? Name
			{
				get;
				set;
			}
		}
	}
}


