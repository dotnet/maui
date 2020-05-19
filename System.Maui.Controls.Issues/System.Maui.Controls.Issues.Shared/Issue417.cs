using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 417, "Navigation.PopToRootAsync does nothing", PlatformAffected.Android)]
	public class Issue417 : TestNavigationPage
	{
		public static NavigationPage NavRoot;

		protected override void Init ()
		{
			Navigation.PushAsync (new FirstPage ());
			NavRoot = this;
		}

		public class FirstPage : ContentPage
		{
			public FirstPage ()
			{
				Title = "First Page";
				BackgroundColor = Color.Black;

				var nextPageBtn = new Button {
					Text = "Next Page"
				};

				nextPageBtn.Clicked += (s, e) => NavRoot.Navigation.PushAsync (new NextPage ());

				Content = nextPageBtn;
			}
		
		}

		public class NextPage : ContentPage
		{
			public NextPage ()
			{
				Title = "Second Page";

				var nextPage2Btn = new Button {
					Text = "Next Page 2"
				};

				nextPage2Btn.Clicked += (s, e) => NavRoot.Navigation.PushAsync (new NextPage2 ());
				BackgroundColor = Color.Black;
				Content = nextPage2Btn;

			}
		}

		public class NextPage2 : ContentPage
		{
			public NextPage2 ()
			{
				Title = "Third Page";

				var popToRootButton = new Button {
					Text = "Pop to root"
				};

				popToRootButton.Clicked += (s, e) => NavRoot.PopToRootAsync ();
				BackgroundColor = Color.Black;
				Content = popToRootButton;
			}
		}


#if UITEST
		[Test]
		[UiTest (typeof(NavigationPage), "PopToRootAsync")]
		public void Issue417TestsNavigateAndPopToRoot ()
		{
			RunningApp.WaitForElement (q => q.Marked ("First Page"));
			RunningApp.WaitForElement (q => q.Button ("Next Page"));
			RunningApp.Screenshot ("All elements present");

			RunningApp.Tap (q => q.Button ("Next Page"));

			RunningApp.WaitForElement (q => q.Marked ("Second Page"));
			RunningApp.WaitForElement (q => q.Button ("Next Page 2"));
			RunningApp.Screenshot ("At second page");
			RunningApp.Tap (q => q.Button ("Next Page 2"));

			RunningApp.WaitForElement (q => q.Marked ("Third Page"));
			RunningApp.WaitForElement (q => q.Button ("Pop to root"));
			RunningApp.Screenshot ("At third page");
			RunningApp.Tap (q => q.Button ("Pop to root"));

			RunningApp.WaitForElement (q => q.Marked ("First Page"));
			RunningApp.WaitForElement (q => q.Button ("Next Page"));
			RunningApp.Screenshot ("All elements present");

			RunningApp.Screenshot ("Popped to root");
		}
#endif
	}


}
