using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1342, "[iOS] ListView throws Exception on ObservableCollection.Add/Remove for non visible list view",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class Issue1342 : TestNavigationPage
	{
		const string add2 = "add2";
		const string add3 = "add3";
		const string success = "No crash means success";

		protected override void Init()
		{
			PushAsync(new MainPageCode
			{
				BindingContext = new MainViewModel
				{
					ViewModel1 = new ListViewModel
					{
						Items = new ObservableCollection<string>(new[] { $"Click {add2}", $"Click {add3}", success })
					},
					ViewModel2 = new ListViewModel
					{
						Items = new ObservableCollection<string>(new[] { "item2.1", "item2.2", "item2.3" })
					},
					ViewModel3 = new ListViewModel
					{
						Items = new ObservableCollection<string>()
					}
				}
			});
		}

		[Preserve(AllMembers = true)]
		public partial class MainPageCode : TabbedPage
		{
			public MainPageCode()
			{
				ToolbarItems.Add(new Xamarin.Forms.ToolbarItem() { Text = "add1" });
				ToolbarItems.Add(new Xamarin.Forms.ToolbarItem() { Text = $"{add2}" });
				ToolbarItems.Add(new Xamarin.Forms.ToolbarItem() { Text = $"{add3}" });
				ToolbarItems.Add(new Xamarin.Forms.ToolbarItem() { Text = "reload" });
				ToolbarItems.Add(new Xamarin.Forms.ToolbarItem() { Text = "visible" });


				ToolbarItems[0].SetBinding(ToolbarItem.CommandProperty, "Add1Command");
				ToolbarItems[1].SetBinding(ToolbarItem.CommandProperty, "Add2Command");
				ToolbarItems[2].SetBinding(ToolbarItem.CommandProperty, "Add3Command");
				ToolbarItems[3].SetBinding(ToolbarItem.CommandProperty, "Add4Command");
				ToolbarItems[4].SetBinding(ToolbarItem.CommandProperty, "Add5Command");

				ListPageCode page = new ListPageCode();
				page.SetBinding(ListPageCode.BindingContextProperty, "ViewModel1");
				Children.Add(page);

				page = new ListPageCode();
				page.SetBinding(ListPageCode.BindingContextProperty, "ViewModel2");
				Children.Add(page);

				page = new ListPageCode();
				page.SetBinding(ListPageCode.BindingContextProperty, "ViewModel3");
				Children.Add(page);
			}
		}

		[Preserve(AllMembers = true)]
		public class MainViewModel
		{

			void AddItems(ObservableCollection<string> list)
			{
				list.Add("new item");
			}

			public MainViewModel()
			{
				Add1Command = new Command(() => AddItems(ViewModel1.Items));
				Add2Command = new Command(() => AddItems(ViewModel2.Items));
				Add3Command = new Command(() => AddItems(ViewModel3.Items));
				Add4Command = new Command(() =>
				{
					ViewModel1.ReloadData();
					ViewModel2.ReloadData();
					ViewModel3.ReloadData();
				});
				Add5Command = new Command(() =>
				{
					ViewModel1.ChangeListViewVisability();
					ViewModel2.ChangeListViewVisability();
					ViewModel3.ReloadData();
				});
			}

			public ListViewModel ViewModel1 { get; set; }
			public ListViewModel ViewModel2 { get; set; }
			public ListViewModel ViewModel3 { get; set; }

			public ICommand Add1Command { get; }
			public ICommand Add2Command { get; }
			public ICommand Add3Command { get; }
			public ICommand Add4Command { get; }
			public ICommand Add5Command { get; }
		}

		[Preserve(AllMembers = true)]
		public class ListViewModel : INotifyPropertyChanged
		{
			public ObservableCollection<string> Items { get; set; }
			public bool IsVisible { get; set; } = true;

			public event PropertyChangedEventHandler PropertyChanged;

			public void ReloadData()
			{
				Items = new ObservableCollection<string>();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
			}

			public void ChangeListViewVisability()
			{
				IsVisible = !IsVisible;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));

			}
		}

		[Preserve(AllMembers = true)]
		public partial class ListPageCode : ContentPage
		{
			public ListPageCode()
			{
				Icon = "coffee.png";
				ListView view = new ListView(ListViewCachingStrategy.RecycleElement);
				Content = view;

				view.SetBinding(ListView.ItemsSourceProperty, "Items");
				view.SetBinding(ListView.IsVisibleProperty, "IsVisible");
			}
		}

#if UITEST
		[Test]
		public void AddingItemsToNonVisibleListViewDoesntCrash()
		{
			RunningApp.Tap(add2);
			RunningApp.Tap(add3);
			RunningApp.WaitForElement(success);
		}
#endif
	}
}
