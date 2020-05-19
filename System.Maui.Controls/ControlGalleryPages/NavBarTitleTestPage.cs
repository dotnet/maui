using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class NavBarTitleTestPage : ContentPage
	{

		public NavBarTitleTestPage ()
		{
			var navTab = new NavigationPage { Title = "Hello 1 nav"};
			navTab.PushAsync (GetPage (navTab));

			var stackPages = new StackLayout ();

			var btn3 = new Button {
				Text = "tab", Command = new Command (async () => {
					var tabbed = new TabbedPage { Title = "Main Tab" };
					tabbed.Children.Add (navTab);
					tabbed.Children.Add (GetPage (navTab));
					await Navigation.PushModalAsync (tabbed);
				})
			};

			var btn4 = new Button {
				Text = "mdp", Command = new Command (async () => {
					var newNav = new NavigationPage { Title = "Hello 1 nav", BarBackgroundColor = Color.Pink, BarTextColor = Color.Blue };
					var mdp = new MasterDetailPage ();
					await newNav.PushAsync (GetPage (newNav));
					mdp.Master = new ContentPage {
						Title = "Master", BackgroundColor = Color.Red, Content = new Button {
							Text = "new", Command = new Command (() => {
								mdp.Detail = new ContactsPage { Title = "hello 3" };
								mdp.IsPresented = false;
							})
						}
					};
					mdp.Detail = newNav;
					await Navigation.PushModalAsync (mdp);
				})
			};

			var btn5 = new Button {
				Text = "nav", Command = new Command (async () => {
					var newNav = new NavigationPage { Title = "Hello 1 nav" };
					await newNav.PushAsync (GetPage (newNav));
					await Navigation.PushModalAsync (newNav);
				})
			};

			var btn6 = new Button {
				Text = "change nav", Command = new Command (() => {
					(Parent as NavigationPage).BarBackgroundColor = Color.Blue;
					(Parent as NavigationPage).BarTextColor = Color.Pink;
				})
			};

			stackPages.Children.Add (btn3);
			stackPages.Children.Add (btn4);
			stackPages.Children.Add (btn5);
			stackPages.Children.Add (btn6);
			Content = stackPages;
		}

		static Page GetPage (NavigationPage navTab)
		{
			var stack = new StackLayout ();
			var newPage = new ContentPage { Title = "Hello 2", Content = stack };
			var btn1 = new Button { Text = "next", Command = new Command (async () => await newPage.Navigation.PushAsync (new ContactsPage { Title = "hello 3" })) };
			var btn2 = new Button {
				Text = "change nav", Command = new Command (() => {
					navTab.BarBackgroundColor = Color.Blue;
					navTab.BarTextColor = Color.Pink;
				})
			};
			var btn3 = new Button {
				Text = "pop modal", Command = new Command (() => {
					newPage.Navigation.PopModalAsync ();
				})
			};
			stack.Children.Add (btn1);
			stack.Children.Add (btn2);
			stack.Children.Add (btn3);
			return newPage;
			
		}
	}
}
