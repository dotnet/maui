using System;
using System.Linq;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 264, "PopModal NRE", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue264 : TestContentPage
	{
		Page _current;

		protected override void Init ()
		{
			var aboutBtn = new Button {
				Text = "About"
			};

			aboutBtn.Clicked += (s, e) => Navigation.PushModalAsync (new AboutPage ());

			var popButton = new Button {
				Text = "Pop me",
				Command = new Command (async () => await Navigation.PopAsync ())
			};

			Content = new StackLayout {
				Children = {
					new Label {Text = "Home"},
					aboutBtn,
					popButton
				}
			};
		}

		// Pop modal null reference exception

#if UITEST
		[Test]
		public void Issue264TestsPushAndPopModal ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Home"));
			RunningApp.WaitForElement (q => q.Button ("About"));
			RunningApp.Screenshot ("All elements present");

			RunningApp.Tap (q => q.Button ("About"));
			RunningApp.WaitForElement (q => q.Button ("Close"));
			RunningApp.Screenshot ("Modal pushed");

			RunningApp.Tap (q => q.Button ("Close"));
			RunningApp.WaitForElement (q => q.Button ("About"));
			RunningApp.Screenshot ("Modal popped");

			RunningApp.Tap (q => q.Button ("Pop me"));
			RunningApp.WaitForElement (q => q.Marked ("Bug Repro's"));
			RunningApp.Screenshot ("No crash");
		}
#endif
	}

	public class AboutPage : ContentPage
	{
		public AboutPage()
		{
			BackgroundColor = Color.Black;
			Content = new Button { Text = "Close", Command = new Command (() => Navigation.PopModalAsync ()) };

		}
	}
}
