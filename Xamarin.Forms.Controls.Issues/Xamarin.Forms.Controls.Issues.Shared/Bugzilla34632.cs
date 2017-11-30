using System;
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
	[Issue (IssueTracker.Bugzilla, 34632, "Can't change IsPresented when setting SplitOnLandscape ")]
	public class Bugzilla34632 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			if (Device.RuntimePlatform == Device.UWP)
				MasterBehavior = MasterBehavior.Split;
			else
				MasterBehavior = MasterBehavior.SplitOnLandscape;

			Master = new ContentPage { Title = "Main Page", 
				Content = new Button { Text = "Master", AutomationId = "btnMaster", 
					Command = new Command (() => {
						//If we're in potrait toggle hide the menu on click
						if (Width < Height || Device.Idiom == TargetIdiom.Phone) {
							IsPresented = false;
						}
					})
				} 
			};

			Detail = new NavigationPage (new ModalRotationIssue ());
			NavigationPage.SetHasBackButton (Detail, false);
		}

		[Preserve (AllMembers = true)]
		public class ModalRotationIssue : ContentPage
		{
			public ModalRotationIssue ()
			{
				var btn = new Button { Text = "Open Modal", AutomationId = "btnModal"  };
				btn.Clicked += OnButtonClicked;
				Content = btn;
			}

			async void OnButtonClicked (object sender, EventArgs e)
			{
				var testButton = new Button { Text = "Rotate Before Clicking", AutomationId = "btnDismissModal" };
				testButton.Clicked += (async (snd, args) => await Navigation.PopModalAsync ());

				var testModal = new ContentPage () {
					Content = testButton
				};

				await Navigation.PushModalAsync (testModal);
			}
		}

		#if UITEST
		[Test]
		public void Bugzilla34632Test ()
		{
			var app = RunningApp as iOSApp;
			if (app != null && app.Device.IsTablet) {
				RunningApp.SetOrientationPortrait ();
				RunningApp.Tap (q => q.Marked ("btnModal"));
				RunningApp.SetOrientationLandscape ();
				RunningApp.Tap (q => q.Marked ("btnDismissModal"));
				RunningApp.Tap (q => q.Marked ("btnModal"));
				RunningApp.SetOrientationPortrait ();
				RunningApp.Tap (q => q.Marked ("btnDismissModal"));
				RunningApp.Tap (q => q.Marked ("btnMaster"));
			}
		}

		[TearDown]
		public void TearDown() 
		{
			RunningApp.SetOrientationPortrait ();
		}
		#endif
	}
}
