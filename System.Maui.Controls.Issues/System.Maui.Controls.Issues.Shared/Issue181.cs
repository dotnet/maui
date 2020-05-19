using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 181, "Color not initialized for Label", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue181 : TestContentPage
	{
		protected override void Init ()
		{
			Title = "Issue 181";
			Content = new Frame {
				BorderColor = Color.Red,
				BackgroundColor = new Color (1.0, 1.0, 0.0),
				Content = new Label {
					Text = "I should have red text",
					TextColor = Color.Red,
					BackgroundColor = new Color (0.5, 0.5, 0.5),
#pragma warning disable 618
					XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
					YAlign = TextAlignment.Center
#pragma warning restore 618
				}
			};
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		[UiTest (typeof(Label), "TextColor")]
		public void Issue181TestsLabelShouldHaveRedText ()
		{
			RunningApp.WaitForElement (q => q.Marked ("I should have red text"));
			RunningApp.Screenshot ("Label should have red text");
		}
#endif
	}
}
