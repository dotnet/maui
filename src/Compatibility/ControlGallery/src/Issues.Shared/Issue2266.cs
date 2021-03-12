using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2266, "Setting a different Detail page from a FlyoutPage after 2nd time on MainPage", PlatformAffected.iOS)]
	public class Issue2266 : TestContentPage
	{
		protected override void Init()
		{
			InitPageContent();
		}

		void InitPageContent()
		{
			var labelHeader = new Label
			{
				Text = "Select a test",
				FontSize = 30,
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			string[] listItems = {
				"MasterDetail Navigation",
				"MasterDetail Navigation ->> Page 1",
				"MasterDetail Navigation ->> Page 2",
				"MasterDetail Navigation ->> Page 3"
			};

			var listView = new ListView
			{
				ItemsSource = listItems
			};

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children = {
					labelHeader,
					listView
				}
			};

			listView.ItemSelected += delegate (object sender, SelectedItemChangedEventArgs e)
			{
				if (e.SelectedItem == null)
					return;
				if (e.SelectedItem.Equals(listItems[0]))
				{
					Application.Current.MainPage = MasterDetailHost;
				}
				else if (e.SelectedItem.Equals(listItems[1]) || e.SelectedItem.Equals(listItems[2]) || e.SelectedItem.Equals(listItems[3]))
				{
					// MasterDetail Navigation - direct page open
					var item = e.SelectedItem.ToString();
					var index = int.Parse(item.Substring(item.Length - 1)) - 1;
					Application.Current.MainPage = MasterDetailHost;
					MasterDetailHost.OpenPage(index);
				}

				listView.SelectedItem = null;
			};
		}

		static MasterDetailNavigation s_masterDetailHost;

		static MasterDetailNavigation MasterDetailHost
		{
			get
			{
				if (s_masterDetailHost == null)
					s_masterDetailHost = new MasterDetailNavigation();
				return s_masterDetailHost;
			}
		}



#if UITEST
		[Test]
		public void SwapMainPageWithMasterDetails()
		{
			RunningApp.WaitForElement(q => q.Text("MasterDetail Navigation"));
			RunningApp.Tap(q => q.Text("MasterDetail Navigation"));
			RunningApp.Tap(q => q.Marked("OpenMaster"));
			RunningApp.Tap(q => q.Text("Page 1"));
			RunningApp.Tap(q => q.Text("START"));
			RunningApp.Tap(q => q.Text("MasterDetail Navigation ->> Page 1"));
			RunningApp.WaitForElement(q => q.Text("Page 1"));
			RunningApp.Tap(q => q.Text("START"));
			RunningApp.Tap(q => q.Text("MasterDetail Navigation ->> Page 2"));
			RunningApp.WaitForElement(q => q.Text("Page 2"));
			RunningApp.Tap(q => q.Text("START"));
			RunningApp.Tap(q => q.Text("MasterDetail Navigation ->> Page 3"));
			RunningApp.WaitForElement(q => q.Text("Page 3"));
		}
#endif

	}

	[Preserve(AllMembers = true)]
	public class MasterDetailNavigation : FlyoutPage
	{
		List<NavigationPage> _pages;

		public MasterDetailNavigation()
		{
			InitPages();

			var menuList = new ListView
			{
				BackgroundColor = Color.Transparent,
				ItemsSource = _pages,
				ItemTemplate = new DataTemplate(typeof(TextCell))
			};
			menuList.ItemTemplate.SetBinding(TextCell.TextProperty, "Title");

			Flyout = new ContentPage
			{
				BackgroundColor = Color.FromHex("363636"),
				Title = "Menu",
				Content = menuList
			};

			Detail = new NavigationPage(new ContentPage
			{
				Padding = new Thickness(20, 20),
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Select a menu item" },
						new Button {Command = new Command(() => this.IsPresented = true), AutomationId = "OpenMaster", Text = "Open Flyout"}
					}
				}
			});

			menuList.ItemSelected += (sender, e) =>
			{
				var page = e.SelectedItem as NavigationPage;
				if (page != null)
				{
					Detail = page;
					IsPresented = false;
				}
			};
		}

		void InitPages()
		{
			_pages = new List<NavigationPage>();

			for (int i = 1; i <= 10; i++)
			{
				var btnSubPage = new Button
				{
					Text = string.Format("Open sub-page"),
				};
				btnSubPage.Clicked += delegate
				{
					OpenSubPage(string.Format("Sub for page: {0}", i));
				};
				var page = new ContentPage
				{
					Padding = new Thickness(20, 20),
					Title = string.Format("Page {0}", i),
					Content = new StackLayout
					{
						Children = {
							new Label { AutomationId = "Page {0}",  Text = string.Format ("Page {0}", i) },
							btnSubPage
						}
					}
				};
				page.ToolbarItems.Add(new ToolbarItem("START", null, delegate
				{
					Application.Current.MainPage = new Issue2266();
				})
				{
					AutomationId = "START"
				});

				_pages.Add(new NavigationPage(page) { Title = page.Title });
			}
		}

		public void OpenPage(int index)
		{
			if (index >= _pages.Count)
			{
				// Index out of range
				return;
			}
			Detail = _pages[index];
		}

		async void OpenSubPage(string text)
		{
			await Detail.Navigation.PushAsync(new ContentPage
			{
				Content = new Label { Text = text }
			});
		}


	}
}


