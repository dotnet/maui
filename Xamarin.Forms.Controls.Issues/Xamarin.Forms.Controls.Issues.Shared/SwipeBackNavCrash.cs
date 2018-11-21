using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
#endif


namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "Swipe back nav crash", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Navigation)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class SwipeBackNavCrash : TestNavigationPage
	{
		protected override void Init ()
		{
			Navigation.PushAsync (new SwipeBackNavCrashPageOne ());
		}

#if UITEST
		[Test]
		public void SwipeBackNavCrashTestsAllElementsPresent ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Page One"));
			RunningApp.WaitForElement (q => q.Button ("Go to second page"));
		}

		[Test]
		public void SwipeBackNavCrashTestsGoToSecondPage () 
		{
			RunningApp.WaitForElement (q => q.Marked ("Page One"));
			RunningApp.Tap (q => q.Button ("Go to second page"));
			RunningApp.Screenshot ("At Second Page");
		}

#if __IOS__
		[Test]
		public void SwipeBackNavCrashTestsSwipeBackDoesNotCrash ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Page One"));
			RunningApp.Tap (q => q.Button ("Go to second page"));
			RunningApp.WaitForElement (q => q.Marked ("Swipe lightly left and right to crash this page"));
			System.Threading.Thread.Sleep (3);

			var mainBounds = RunningApp.RootViewRect();

			Xamarin.Forms.Core.UITests.Gestures.Pan (RunningApp, new Xamarin.Forms.Core.UITests.Drag (mainBounds, 0, 125, 75, 125, Xamarin.Forms.Core.UITests.Drag.Direction.LeftToRight));
			System.Threading.Thread.Sleep (3);
			RunningApp.Screenshot ("Crash?");
			RunningApp.WaitForElement (q => q.Marked ("Swipe lightly left and right to crash this page"));
		}
#endif
#endif

	}
	
	public class SwipeBackNavCrashPageOne : ContentPage
	{
		public SwipeBackNavCrashPageOne ()
		{
			Title = "Page One";
			var button = new Button {
				Text = "Go to second page"
			};
			button.Clicked += (sender, e) => Navigation.PushAsync (new PageTwo ());
			Content = button;
		}
	}

	public class PageTwo : ContentPage
	{
		public PageTwo ()
		{
			Title = "Second Page";
			Content = new StackLayout {
				Children = {
					new Label { Text = "Swipe lightly left and right to crash this page" },
					new BoxView { Color = new Color (0.0) }
				}
			}; 
		}
	}
}
