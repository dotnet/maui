using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest.Android;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 28570, "https://bugzilla.xamarin.com/show_bug.cgi?id=28570")]
	internal class Bugzilla28570 : TestContentPage
	{
		public ScrollView ScrollView;

		protected override void Init ()
		{
			Label header = new Label {
				Text = "ScrollView Bug",
				FontSize = 50,
				FontAttributes = FontAttributes.Bold,
				HorizontalOptions = LayoutOptions.Center
			};

			Label lab1 = new Label ();
			lab1.Text = "Sometimes page content fits entirely on " +
						"the page. That's very convenient. But " +
						"on many occasions, the content of the page " +
						"is much too large for the page, or only " +
						"becomes available at runtime." +
						"\n\n" +
						"For cases such as these, the ScrollView " +
						"provides a solution. Simply set its " +
						"Content property to your content \u2014 in this " +
						"case a Label but in the general case very " +
						"likely a Layout derivative with multiple " +
						"children \u2014 and the ScrollView provides " +
						"scrolling with the distinctive look and touch " +
						"familiar to the user." +
						"\n\n" +
						"The ScrollView is also capable of " +
						"horizontal scrolling, and while that's " +
						"usually not as common as vertical scrolling, " +
						"sometimes it comes in handy." +
						"\n\n" +
						"Most often, the content of a ScrollView is " +
						"a StackLayout. Whenever you're using a " +
						"StackLayout with a number of items determined " +
						"only at runtime, you should probably put it in " +
						"a StackLayout just to be sure your stuff doesn't " +
						"go running off the bottom of the screen." +
						"Most often, the content of a ScrollView is " +
						"a StackLayout. Whenever you're using a " +
						"StackLayout with a number of items determined " +
						"only at runtime, you should probably put it in " +
						"a StackLayout just to be sure your stuff doesn't " +
						"go running off the bottom of the screen.";

			var targetLabel = new Label {Text = "Find Me"};
			targetLabel.AutomationId = "28570Target";

			lab1.FontSize = Device.GetNamedSize (NamedSize.Small, typeof (Label));


			ScrollView = new ScrollView {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Content = new StackLayout {
					Children = {
						lab1,
						targetLabel
					}
				}
			};

			Button makeBig = new Button ();
			makeBig.Text = "Tap";
			//
			// Clicking button first time does not scroll event though scrollView.Height is already set.
			// Clicking a second time does correctly scroll to the end.  scrollView.Height  is unchanged.
			//
			// For this test to work you should make sure the text fits into the screen when the font is small
			// and then becomes larger than the screeen when switching to the Large font.
			//
			makeBig.Clicked += (object sender, EventArgs e) => {
				lab1.FontSize = Device.GetNamedSize (NamedSize.Large, typeof (Label));
				Debug.WriteLine ("******** scrollView.Height= {0}", lab1.Height); // this shows the same updated size on all clicks, so this is not the problem.
				ScrollView.ScrollToAsync (0, lab1.Bounds.Bottom, false);
			};

			// Build the page.
			Content = new StackLayout {
				Children = {
					makeBig,
					header,
					ScrollView,
				}
			};
		}

#if UITEST
		[Test]
		[Ignore("Fails intermittently on TestCloud")]
		public void Bugzilla28570Test ()
		{
#if __ANDROID__
			RunningApp.WaitForElement (q => q.Marked ("Tap"));
			RunningApp.Screenshot ("At test page");
			RunningApp.Tap (q => q.Marked ("Tap"));

			RunningApp.WaitForElement (q => q.Marked ("28570Target"));
#endif
		}
#endif
	}
}