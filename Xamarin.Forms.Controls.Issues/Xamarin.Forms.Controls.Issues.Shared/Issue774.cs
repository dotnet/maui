using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 774, "ActionSheet won't dismiss after rotation to landscape", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue774 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new StackLayout {
				Children = {
					new Label {
						Text = "Hi"
					},
					new Button {
						Text = "Show ActionSheet",
						Command = new Command (async () => await DisplayActionSheet ("What's up", "Dismiss", "Destroy"))
					}
				}
			};
		}

#if UITEST
		[Test]
		public void Issue774TestsDismissActionSheetAfterRotation ()
		{
			RunningApp.Tap(q => q.Button("Show ActionSheet"));
			RunningApp.Screenshot("Show ActionSheet");

			RunningApp.SetOrientationLandscape();
			RunningApp.Screenshot("Rotate Device");

			// Wait for the action sheet element to show up
			RunningApp.WaitForElement(q => q.Marked("What's up"));

			var dismiss = RunningApp.Query("Dismiss");

			var target = dismiss.Length > 0 ? "Dismiss" : "Destroy";


			RunningApp.Tap(q => q.Marked(target));
			RunningApp.WaitForNoElement(q => q.Marked(target));

			RunningApp.Screenshot("Dismiss ActionSheet");

		}

		[TearDown]
		public void TearDown()
		{
			RunningApp.SetOrientationPortrait();
		}
#endif

	}
}
