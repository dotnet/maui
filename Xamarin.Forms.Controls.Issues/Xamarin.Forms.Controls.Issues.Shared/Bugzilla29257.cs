using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	// Note that this test currently fails on UWP because of https://bugzilla.xamarin.com/show_bug.cgi?id=60478

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 29257, "CarouselPage.CurrentPage Does Not Work Properly When Used Inside a NavigationPage ")]
	public class Bugzilla29257 : TestContentPage 
	{
		List<string> _menuItems = new List<string> {
			"Page 1", "Page 2", "Page 3", "Page 4", "Page 5"
		};

		ListView _menu;

		protected override void Init ()
		{
			_menu = new ListView { ItemsSource = _menuItems };

			_menu.ItemSelected += PageSelected;

			Content = _menu;
		}

		async void PageSelected(object sender, SelectedItemChangedEventArgs e)
		{
			var selection = e.SelectedItem as string;

			switch (selection)
			{
			case "Page 1":
				await Navigation.PushAsync(new TestPage(0));
				break;

			case "Page 2":
				await Navigation.PushAsync(new TestPage(1));
				break;

			case "Page 3":
				await Navigation.PushAsync(new TestPage(2));
				break;

			case "Page 4":
				await Navigation.PushAsync(new TestPage(3));
				break;

			case "Page 5":
				await Navigation.PushAsync(new TestPage(4));
				break;
			}
			_menu.SelectedItem = null;
		}

		public class TestPage : CarouselPage
		{
			public TestPage()
			{
				Children.Add(new ContentPage { Content = new Label { Text = "This is page 1" , BackgroundColor = Color.Red} });
				Children.Add(new ContentPage { Content = new Label { Text = "This is page 2" , BackgroundColor = Color.Green} });
				Children.Add(new ContentPage { Content = new Label { Text = "This is page 3" , BackgroundColor = Color.Blue} });
				Children.Add(new ContentPage { Content = new Label { Text = "This is page 4" , BackgroundColor = Color.Pink} });
				Children.Add(new ContentPage { Content = new Label { Text = "This is page 5" , BackgroundColor = Color.Yellow } });

			}

			public TestPage(int page) : this()
			{
				CurrentPage = Children[page];
			}
		}

#if UITEST
		[Test]
		public void Bugzilla29257Test ()
		{
			RunningApp.Tap (q => q.Marked ("Page 1"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.WaitForElement (q => q.Marked ("This is page 1"));
			RunningApp.Back ();
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.Tap (q => q.Marked ("Page 2"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.WaitForElement (q => q.Marked ("This is page 2"));
			RunningApp.Back ();
			RunningApp.Tap (q => q.Marked ("Page 3"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.WaitForElement (q => q.Marked ("This is page 3"));
			RunningApp.Back ();
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.Tap (q => q.Marked ("Page 4"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.WaitForElement (q => q.Marked ("This is page 4"));
			RunningApp.Back ();
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.Tap (q => q.Marked ("Page 5"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif
			RunningApp.WaitForElement (q => q.Marked ("This is page 5"));
		}
#endif
	}
}
