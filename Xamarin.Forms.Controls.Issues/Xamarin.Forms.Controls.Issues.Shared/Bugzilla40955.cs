using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40955, "Memory leak with FormsAppCompatActivity and NavigationPage", PlatformAffected.Android)]
	public class Bugzilla40955 : TestMasterDetailPage
	{
		const string DestructorMessage = "NavigationPageEx Destructor called";
		const string Page1Title = "Page1";
		const string Page2Title = "Page2";
		const string Page3Title = "Page3";

		protected override void Init()
		{
			var masterPage = new MasterPage();
			Master = masterPage;
			masterPage.ListView.ItemSelected += (sender, e) =>
			{
				var item = e.SelectedItem as MasterPageItem;
				if (item != null)
				{
					Detail = new NavigationPageEx((Page)Activator.CreateInstance(item.TargetType));
					masterPage.ListView.SelectedItem = null;
					IsPresented = false;
				}
			};

			Detail = new NavigationPageEx(new _409555_Page1());
		}

		[Preserve(AllMembers = true)]
		public class MasterPageItem
		{
			public string IconSource { get; set; }

			public Type TargetType { get; set; }

			public string Title { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class MasterPage : ContentPage
		{
			public MasterPage()
			{
				Title = "Menu";
				ListView = new ListView { VerticalOptions = LayoutOptions.FillAndExpand, SeparatorVisibility = SeparatorVisibility.None };

				ListView.ItemTemplate = new DataTemplate(() =>
				{
					var ic = new ImageCell();
					ic.SetBinding(TextCell.TextProperty, "Title");
					return ic;
				});

				Content = new StackLayout
				{
					Children = { ListView }
				};

				var masterPageItems = new List<MasterPageItem>();
				masterPageItems.Add(new MasterPageItem
				{
					Title = Page1Title,
					TargetType = typeof(_409555_Page1)
				});
				masterPageItems.Add(new MasterPageItem
				{
					Title = Page2Title,
					TargetType = typeof(_409555_Page2)
				});
				masterPageItems.Add(new MasterPageItem
				{
					Title = Page3Title,
					TargetType = typeof(_409555_Page3)
				});

				ListView.ItemsSource = masterPageItems;
			}

			public ListView ListView { get; }
		}

		[Preserve(AllMembers = true)]
		public class NavigationPageEx : NavigationPage
		{
			public NavigationPageEx(Page root) : base(root)
			{
			}

			~NavigationPageEx()
			{
				Debug.WriteLine(DestructorMessage);
			}
		}

		[Preserve(AllMembers = true)]
		public class _409555_Page1 : ContentPage
		{
			public _409555_Page1()
			{
				Title = Page1Title;
				Content = new StackLayout { Children = { new Label { Text = "Open the drawer menu and select Page2" } } };
			}
		}

		[Preserve(AllMembers = true)]
		public class _409555_Page2 : ContentPage
		{
			public _409555_Page2()
			{
				Title = Page2Title;
				Content = new StackLayout { Children = { new Label { Text = "Open the drawer menu and select Page3" } } };
			}
		}

		[Preserve(AllMembers = true)]
		public class _409555_Page3 : ContentPage
		{
			public _409555_Page3()
			{
				Title = Page3Title;
				Content = new StackLayout { Children = { new Label { Text = $"The console should have displayed the text '{DestructorMessage}' at least once. If not, this test has failed." } } };
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				GC.Collect();
				GC.Collect();
				GC.Collect();
			}
		}
	}
}