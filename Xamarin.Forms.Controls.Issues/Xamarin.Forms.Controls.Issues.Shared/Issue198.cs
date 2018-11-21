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
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 198, "TabbedPage shouldn't proxy content of NavigationPage", PlatformAffected.iOS)]
	public class Issue198 : TestTabbedPage
	{
		protected override void Init ()
		{
			Title = "Tabbed Navigation Page";

			var leavePageBtn = new Button {
				Text = "Leave"
			};

			// Should work as expected, however, causes NRE
			leavePageBtn.Clicked += (s, e) => Navigation.PopModalAsync ();

			var navigationPageOne = new NavigationPage (new ContentPage {
				Icon = "calculator.png",
				Content = leavePageBtn
			}) {
				Title = "Page One",
			};
			var navigationPageTwo = new NavigationPage (new ContentPage {
				Icon = "calculator.png",
			}) {
				Title = "Page Two",
			};
			var navigationPageThree = new NavigationPage (new ContentPage {
				Title = "No Crash",
			}) {
				Title = "Page Three",
				Icon = "calculator.png"
			};
			var navigationPageFour = new NavigationPage (new ContentPage ()) {
				Title = "Page Four",
				Icon = "calculator.png"
			};

			Children.Add (navigationPageOne);
			Children.Add (navigationPageTwo);
			Children.Add (navigationPageThree);
			Children.Add (navigationPageFour);
		}

#if UITEST
		[Test]
		[UiTest (typeof(TabbedPage))]
		public void Issue198TestsNREWithPopModal ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Page One"));
			RunningApp.WaitForElement (q => q.Button ("Leave"));
			RunningApp.Screenshot ("All Elements Present");

			RunningApp.Tap (q => q.Marked ("Leave"));
			RunningApp.Screenshot ("Clicked Leave");

			RunningApp.WaitForElement (q => q.Marked ("Bug Repro's"));
#if !__MACOS__
			RunningApp.ClearText(q => q.Raw("* marked:'SearchBarGo'"));
			RunningApp.EnterText(q => q.Raw("* marked:'SearchBarGo'"), "G198");
#endif
			RunningApp.Tap (q => q.Marked ("SearchButton"));
			RunningApp.Screenshot ("Navigate into gallery again");

			RunningApp.WaitForElement (q => q.Marked ("Page Three"));
			RunningApp.Tap (q => q.Marked ("Page Three"));

			RunningApp.WaitForElement (q => q.Marked ("No Crash"));
			RunningApp.Screenshot ("App did not crash");
		}
#endif

	}
}
