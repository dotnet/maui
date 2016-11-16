using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.Android;
using System.Collections.Generic;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 30317, "https://bugzilla.xamarin.com/show_bug.cgi?id=30137")]
	public class Bugzilla30317 : TestNavigationPage // or TestMasterDetailPage, etc ...
	{
		[Preserve (AllMembers = true)]
		public class Bugzilla30317ListItem 
		{
			public string Label { get; set; }
		}

		[Preserve (AllMembers = true)]
		public class Bugzilla30317ListCell : ViewCell
		{
			public Bugzilla30317ListCell()
			{
				var label = new Label (); ;
				label.SetBinding(Label.TextProperty, "Label");
				View = label;
			}
		}

		[Preserve (AllMembers = true)]
		public class Bugzilla30317Page1 : ContentPage
		{
			ListView _listView;

			public Bugzilla30317Page1 ()
			{
				AutomationId = "PageOne";
				Title = "Set ItemSource On Appearing";

				_listView = new ListView ();

				_listView.ItemTemplate = new DataTemplate(typeof(Bugzilla30317ListCell));

				var nextPageButton = new Button {
					AutomationId = "GoToPageTwoButton",
					Text = "Go Page 2",
					Command = new Command (async () => {
						await Navigation.PushAsync (new Bugzilla30317Page2 ());
					})
				};

				Content =  new StackLayout { Children = { nextPageButton, _listView } };
			}

			protected override void OnAppearing ()
			{
				base.OnAppearing ();

				_listView.ItemsSource = new Bugzilla30317ListItem[] {
					new Bugzilla30317ListItem { Label = "PageOneItem1" },
					new Bugzilla30317ListItem { Label = "PageOneItem2" },
					new Bugzilla30317ListItem { Label = "PageOneItem3" },
					new Bugzilla30317ListItem { Label = "PageOneItem4" },
					new Bugzilla30317ListItem { Label = "PageOneItem5" },
				};
			}
		}

		[Preserve (AllMembers = true)]
		public class Bugzilla30317Page2 : ContentPage
		{
			public Bugzilla30317Page2 ()
			{
				AutomationId = "PageTwo";
				Title = "Set ItemSource in ctor";

				var listView = new ListView ();

				listView.ItemTemplate = new DataTemplate(typeof(Bugzilla30317ListCell));
				listView.ItemsSource = new Bugzilla30317ListItem[] {
					new Bugzilla30317ListItem { Label = "PageTwoItem1" },
					new Bugzilla30317ListItem { Label = "PageTwoItem2" },
					new Bugzilla30317ListItem { Label = "PageTwoItem3" },
					new Bugzilla30317ListItem { Label = "PageTwoItem4" },
					new Bugzilla30317ListItem { Label = "PageTwoItem5" },
				};

				var nextPageButton = new Button {
					AutomationId = "GoToPageThreeButton",
					Text = "Go Page 3",
					Command = new Command (async () => {
						await Navigation.PushModalAsync (new Bugzilla30317Page3 ());
					})
				};

				Content =  new StackLayout { Children = { nextPageButton, listView } };
			}
		}

		[Preserve (AllMembers = true)]
		public class Bugzilla30317Page3TabOne : ContentPage
		{
			public Bugzilla30317Page3TabOne ()
			{
				AutomationId = "TabbedPageOne";
				Title = "TabOneCtor";

				var listView = new ListView ();

				listView.ItemTemplate = new DataTemplate(typeof(Bugzilla30317ListCell));
				listView.ItemsSource = new Bugzilla30317ListItem[] {
					new Bugzilla30317ListItem { Label = "PageThreeTabOneItem1" },
					new Bugzilla30317ListItem { Label = "PageThreeTabOneItem2" },
					new Bugzilla30317ListItem { Label = "PageThreeTabOneItem3" },
					new Bugzilla30317ListItem { Label = "PageThreeTabOneItem4" },
					new Bugzilla30317ListItem { Label = "PageThreeTabOneItem5" },
				};

				Content = listView;
			}
		}

		[Preserve (AllMembers = true)]
		public class Bugzilla30317Page3TabTwo : ContentPage
		{
			ListView _listView;

			public Bugzilla30317Page3TabTwo ()
			{
				AutomationId = "TabbedPageTwo";
				Title = "TabTwoOnAppearing";

				_listView = new ListView ();

				_listView.ItemTemplate = new DataTemplate(typeof(Bugzilla30317ListCell));
				

				Content = _listView;

			}

			protected override void OnAppearing ()
			{
				base.OnAppearing ();

				_listView.ItemsSource = new Bugzilla30317ListItem[] {
					new Bugzilla30317ListItem { Label = "PageThreeTabTwoItem1" },
					new Bugzilla30317ListItem { Label = "PageThreeTabTwoItem2" },
					new Bugzilla30317ListItem { Label = "PageThreeTabTwoItem3" },
					new Bugzilla30317ListItem { Label = "PageThreeTabTwoItem4" },
					new Bugzilla30317ListItem { Label = "PageThreeTabTwoItem5" },
				};
			}
		}

		[Preserve (AllMembers = true)]
		public class Bugzilla30317Page3 : TabbedPage
		{
			public Bugzilla30317Page3 ()
			{
				Children.Add (new Bugzilla30317Page3TabOne ());
				Children.Add (new Bugzilla30317Page3TabTwo ());
			}
		}

		protected override void Init ()
		{
			Navigation.PushAsync (new Bugzilla30317Page1 ());
		}

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla30317ItemSourceOnAppearingContentPage ()
		{
			RunningApp.Screenshot ("I am at Bugzilla30317");
			RunningApp.WaitForElement (q => q.Marked ("PageOne"));
			RunningApp.Screenshot ("I see Page 1");
	
			RunningApp.WaitForElement (q => q.Marked ("PageOneItem1"));
			RunningApp.TouchAndHold (q => q.Marked ("PageOneItem1"));
		
			RunningApp.WaitForElement (q => q.Marked ("PageOneItem5"));
			RunningApp.TouchAndHold (q => q.Marked ("PageOneItem5"));

			RunningApp.Screenshot ("I did not crash");
		}

		[Test]
		public void Bugzilla30317ItemSourceCtorContentPage ()
		{
			RunningApp.WaitForElement (q => q.Marked ("GoToPageTwoButton"));
			RunningApp.Tap (q => q.Marked ("GoToPageTwoButton"));

			RunningApp.WaitForElement (q => q.Marked ("PageTwo"));
			RunningApp.Screenshot ("I see Page 2");
				
			RunningApp.WaitForElement (q => q.Marked ("PageTwoItem1"));
			RunningApp.TouchAndHold (q => q.Marked ("PageTwoItem1"));
				
			RunningApp.WaitForElement (q => q.Marked ("PageTwoItem5"));
			RunningApp.TouchAndHold (q => q.Marked ("PageTwoItem5"));
				
			RunningApp.Screenshot ("I did not crash");
		}

		[Test]
		public void Bugzilla30317ItemSourceTabbedPage ()
		{
			RunningApp.WaitForElement (q => q.Marked ("GoToPageTwoButton"));
			RunningApp.Tap (q => q.Marked ("GoToPageTwoButton"));

			RunningApp.Screenshot ("I see Page 2");
			RunningApp.WaitForElement (q => q.Marked ("PageTwo"));

			RunningApp.WaitForElement (q => q.Marked ("GoToPageThreeButton"));
			RunningApp.Tap (q => q.Marked ("GoToPageThreeButton"));

			RunningApp.Screenshot ("I see TabbedPage One");
			RunningApp.WaitForElement (q => q.Marked ("TabOneCtor"));

			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabOneItem1"));
			RunningApp.TouchAndHold (q => q.Marked ("PageThreeTabOneItem1"));
			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabOneItem1"));

			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabOneItem5"));
			RunningApp.TouchAndHold (q => q.Marked ("PageThreeTabOneItem5"));
			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabOneItem5"));

			RunningApp.Screenshot ("I see TabbedPage Two");
			RunningApp.WaitForElement (q => q.Marked ("TabTwoOnAppearing"));
			RunningApp.Tap (q => q.Marked ("TabTwoOnAppearing"));

			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabTwoItem1"));
			RunningApp.TouchAndHold (q => q.Marked ("PageThreeTabTwoItem1"));
			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabTwoItem1"));

			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabTwoItem5"));
			RunningApp.TouchAndHold (q => q.Marked ("PageThreeTabTwoItem5"));
			RunningApp.WaitForElement (q => q.Marked ("PageThreeTabTwoItem5"));
		}
#endif
	}
}
