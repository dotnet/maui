using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
	[Issue(IssueTracker.Github, 3089, "TextCell text doesn't change when using Recycling on ListViews")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class Issue3089 : TestNavigationPage
	{
		const string reload = "reload";
		const string success = "success";

		protected override void Init()
		{
			var oc = new ObservableCollection<string>(new[] { $"Click {reload}","and this text should go away" });

			Enumerable.Range(0, 100).ForEach(x => oc.Add(x.ToString()));

			PushAsync(new MainPageCode
			{
				BindingContext = new MainViewModel
				{
					ViewModel1 = new ListViewModel
					{
						Items = oc
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

				ToolbarItems.Add(new Xamarin.Forms.ToolbarItem() { Text = "reload" });

				ToolbarItems[0].SetBinding(ToolbarItem.CommandProperty, "Add1Command");
				ToolbarItems[1].SetBinding(ToolbarItem.CommandProperty, "Add2Command");

				ListPageCode page = new ListPageCode();
				page.SetBinding(ListPageCode.BindingContextProperty, "ViewModel1");
				Children.Add(page);
			}
		}

		[Preserve(AllMembers = true)]
		public class MainViewModel
		{

			void AddItems(ObservableCollection<string> list)
			{
				list.Add("new item");
				list.Add(success);
			}

			public MainViewModel()
			{
				Add1Command = new Command(() => AddItems(ViewModel1.Items));
				Add2Command = new Command(() =>
				{
					ViewModel1.ReloadData();
					AddItems(ViewModel1.Items);
				});
			}

			public ListViewModel ViewModel1 { get; set; }

			public ICommand Add1Command { get; }
			public ICommand Add2Command { get; }
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
			}
		}

#if UITEST
		[Test]
		public void ResettingItemsOnRecycledListViewKeepsOldText()
		{
			RunningApp.Tap(reload);
			RunningApp.WaitForElement(success);
		}
#endif
	}
}
