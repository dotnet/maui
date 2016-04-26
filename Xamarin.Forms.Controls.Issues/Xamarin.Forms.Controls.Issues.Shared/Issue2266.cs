using System;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2266, "Setting a different Detail page from a MasterDetailPage after 2nd time on MainPage", PlatformAffected.iOS, NavigationBehavior.SetApplicationRoot)]
	public class Issue2266 : ContentPage
	{
		public Issue2266 ()
		{
			InitPageContent ();
		}

		void InitPageContent ()
		{
			var labelHeader = new Label {
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

			var listView = new ListView {
				ItemsSource = listItems
			};

			Content = new StackLayout {
				Padding = new Thickness (0, 20, 0, 0),
				Children = {
					labelHeader,
					listView
				}
			};

			listView.ItemSelected += delegate(object sender, SelectedItemChangedEventArgs e) {
				if (e.SelectedItem == null)
					return;
				if (e.SelectedItem.Equals (listItems [0])) {
					Application.Current.MainPage = MasterDetailHost;
				} else if (e.SelectedItem.Equals (listItems [1]) || e.SelectedItem.Equals (listItems [2]) || e.SelectedItem.Equals (listItems [3])) {
					// MasterDetail Navigation - direct page open
					var item = e.SelectedItem.ToString ();
					var index = int.Parse (item.Substring (item.Length - 1)) - 1;
					Application.Current.MainPage = MasterDetailHost;
					MasterDetailHost.OpenPage (index);
				}

				listView.SelectedItem = null;
			};
		}

		static MasterDetailNavigation s_masterDetailHost;

		public static MasterDetailNavigation MasterDetailHost {
			get {
				if (s_masterDetailHost == null)
					s_masterDetailHost = new MasterDetailNavigation ();
				return s_masterDetailHost;
			}
		}

	}

	public class MasterDetailNavigation : MasterDetailPage
	{
		List<NavigationPage> _pages;

		public MasterDetailNavigation ()
		{
			InitPages ();

			var menuList = new ListView {
				BackgroundColor = Color.Transparent,
				ItemsSource = _pages,
				ItemTemplate = new DataTemplate (typeof(TextCell))
			};
			menuList.ItemTemplate.SetBinding (TextCell.TextProperty, "Title");

			Master = new ContentPage {
				BackgroundColor = Color.FromHex ("363636"),
				Title = "Menu",
				Content = menuList
			};

			Detail = new NavigationPage (new ContentPage { 
				Padding = new Thickness (20, 20),
				Content = new StackLayout { 
					Children = {
						new Label { Text = "Select a menu item" }
					}
				}
			});

			menuList.ItemSelected += (sender, e) => {
				var page = e.SelectedItem as NavigationPage;
				if (page != null) {
					Detail = page;
					IsPresented = false;
				}
			};
		}

		void InitPages ()
		{
			_pages = new List<NavigationPage> ();

			for (int i = 1; i <= 10; i++) {
				var btnSubPage = new Button { 
					Text = string.Format ("Open sub-page"), 
				};
				btnSubPage.Clicked += delegate {
					OpenSubPage (string.Format ("Sub for page: {0}", i));
				};
				var page = new ContentPage {
					Padding = new Thickness (20, 20),
					Title = string.Format ("Page {0}", i),
					Content = new StackLayout {
						Children = {
							new Label { Text = string.Format ("Page {0}", i) },
							btnSubPage
						}
					}
				};
				page.ToolbarItems.Add (new ToolbarItem ("START", null, delegate {
					Application.Current.MainPage = App.MenuPage;
				}));
				_pages.Add (new NavigationPage (page) { Title = page.Title });
			}
		}

		public void OpenPage (int index)
		{
			if (index >= _pages.Count) {
				// Index out of range
				return;
			}
			Detail = _pages [index];
		}

		async void OpenSubPage (string text)
		{
			await Detail.Navigation.PushAsync (new ContentPage {
				Content = new Label { Text = text }
			});
		}
	}
#endif
}


