using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11969,
		"[Bug] Disabling Swipe view not handling tap gesture events on the content in iOS of Xamarin Forms",
		PlatformAffected.iOS)]
	public partial class Issue11969 : TestContentPage
	{
		const string SwipeViewId = "SwipeViewId";
		const string SwipeButtonId = "SwipeButtonId";

		const string Failed = "SwipeView Button not tapped";
		const string Success = "SUCCESS";

		public Issue11969()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if APP
		void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Debug.WriteLine("CollectionView SelectionChanged");
		}

		void OnSwipeViewSwipeStarted(object sender, SwipeStartedEventArgs e)
		{
			Debug.WriteLine("SwipeView SwipeStarted");
		}

		void OnSwipeViewSwipeEnded(object sender, SwipeEndedEventArgs e)
		{
			Debug.WriteLine("SwipeView SwipeEnded");
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			Debug.WriteLine("Button Clicked");
			TestLabel.Text = Success;
			DisplayAlert("Issue 11969", "Button Clicked", "Ok");
		}
#endif

#if UITEST && __IOS__
		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeDisableChildButtonTest()
		{
			RunningApp.WaitForElement(q => q.Marked(Failed));
			RunningApp.WaitForElement(SwipeViewId);
			RunningApp.Tap("SwipeViewCheckBoxId");
			RunningApp.Tap("SwipeViewContentCheckBoxId");
			RunningApp.Tap(SwipeButtonId);
			RunningApp.WaitForElement(q => q.Marked(Success));
			RunningApp.Tap("Ok");
		}
#endif
	}
}