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

			RunningApp.Tap (q => q.Button ("Show ActionSheet"));
			RunningApp.Screenshot ("Show ActionSheet");

			RunningApp.SetOrientationLandscape ();
			RunningApp.Screenshot ("Rotate Device");
		
			var app = (RunningApp as iOSApp);

			if (app != null) {

				if (!app.Device.IsTablet)
					RunningApp.Tap (q => q.Marked ("Dismiss"));
				else // iPad does not have dismiss option
					RunningApp.Tap (q => q.Marked ("Destroy"));

				if(app.Device.IsTablet)
					RunningApp.WaitForNoElement (q => q.Marked ("Destroy"));
				else
					RunningApp.WaitForNoElement (q => q.Marked ("Dismiss"));

				RunningApp.Screenshot ("Dismiss ActionSheet");

//				App.SetOrientationPortrait ();
//				App.Tap (q => q.Button ("Show ActionSheet"));
//				App.Screenshot ("Rotate and show ActionSheet");
//
//				if (!app.Device.IsTablet)
//					App.Tap (q => q.Button ("Dismiss"));
//				else // iPad does not have dismiss option
//					App.Tap (q => q.Marked ("Destroy"));
//
//				if (app.Device.IsTablet)
//					App.WaitForNoElement (q => q.Marked ("Destroy"));
//				else // iPad does not have dismiss option
//					App.WaitForNoElement (q => q.Marked ("Dismiss"));
			
			} 
			else
			{
				RunningApp.Tap(q => q.Marked("Dismiss"));
			}
		}

		[TearDown]
		public void TearDown()
		{
			RunningApp.SetOrientationPortrait();
		}
#endif

	}
}
