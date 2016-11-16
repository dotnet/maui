using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;

#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 31366, "Pushing and then popping a page modally causes ArgumentOutOfRangeException",
		PlatformAffected.All)]
	public class Bugzilla31366 : TestNavigationPage
	{
		protected override void Init ()
		{
			var page1 = new ContentPage () { Title = "Page1" };

			var successLabel = new Label ();
			var startPopOnAppearing = new Button () { Text = "Start PopOnAppearing Test" };
			var startModalStack = new Button () { Text = "Start ModalStack Test" };

			page1.Content = new StackLayout () {
				Children = { startPopOnAppearing, startModalStack, successLabel }
			};

			var popOnAppearing = new ContentPage () {
				Title = "PopOnAppearing",
				Content = new StackLayout ()
			};

			popOnAppearing.Appearing += async (sender, args) => {
				await Task.Yield ();
			    await popOnAppearing.Navigation.PopModalAsync ();
			};

			startPopOnAppearing.Clicked += async (sender, args) => {
				successLabel.Text = string.Empty;

				await page1.Navigation.PushModalAsync (popOnAppearing);

				successLabel.Text = "If this is visible, the PopOnAppearing test has passed.";
			};

			startModalStack.Clicked += async (sender, args) => {
				successLabel.Text = string.Empty;

				var intermediatePage = new ContentPage () {
					Content = new StackLayout () {
						Children = {
							new Label () { Text = "If this is visible, the modal stack test has passed." }
						}
					}
				};

				await intermediatePage.Navigation.PushModalAsync (popOnAppearing);

				await page1.Navigation.PushModalAsync (intermediatePage);
			};

			PushAsync (page1);
		}


#if UITEST
		[Test]
		[UiTest (typeof (NavigationPage))]
		public void Issue31366PushingAndPoppingModallyCausesArgumentOutOfRangeException ()
		{
			RunningApp.Tap (q => q.Marked ("Start PopOnAppearing Test"));
			RunningApp.WaitForElement (q => q.Marked ("If this is visible, the PopOnAppearing test has passed."));
		}

		[Test]
		[UiTest (typeof (NavigationPage))]
		public void Issue31366PushingWithModalStackCausesIncorrectStackOrder ()
		{
			RunningApp.Tap (q => q.Marked ("Start ModalStack Test"));
			RunningApp.WaitForElement (q => q.Marked ("If this is visible, the modal stack test has passed."));
		}
#endif
	}
}