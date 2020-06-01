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
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 34632, "Can't change IsPresented when setting SplitOnLandscape ")]
	public class Bugzilla34632 : TestMasterDetailPage
	{
		protected override void Init()
		{
			if (Device.RuntimePlatform == Device.UWP)
				MasterBehavior = MasterBehavior.Split;
			else
				MasterBehavior = MasterBehavior.SplitOnLandscape;

			Master = new ContentPage
			{
				Title = "Main Page",
				Content = new Button
				{
					Text = "Master",
					AutomationId = "btnMaster",
					Command = new Command(() =>
					{
						//If we're in potrait toggle hide the menu on click
						if (Width < Height || Device.Idiom == TargetIdiom.Phone)
						{
							IsPresented = false;
						}
					})
				}
			};

			Detail = new NavigationPage(new ModalRotationIssue());
			NavigationPage.SetHasBackButton(Detail, false);
		}

		[Preserve(AllMembers = true)]
		public class ModalRotationIssue : ContentPage
		{
			public ModalRotationIssue()
			{
				var btn = new Button { Text = "Open Modal", AutomationId = "btnModal" };
				btn.Clicked += OnButtonClicked;
				Content = btn;
			}

			async void OnButtonClicked(object sender, EventArgs e)
			{
				var testButton = new Button { Text = "Rotate Before Clicking", AutomationId = "btnDismissModal" };
				testButton.Clicked += (async (snd, args) => await Navigation.PopModalAsync());

				var testModal = new ContentPage()
				{
					Content = testButton
				};

				await Navigation.PushModalAsync(testModal);
			}
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla34632Test()
		{
			if (RunningApp.IsTablet())
			{
				RunningApp.SetOrientationPortrait();
				RunningApp.Tap(q => q.Marked("btnModal"));
				RunningApp.SetOrientationLandscape();
				RunningApp.Tap(q => q.Marked("btnDismissModal"));
				RunningApp.Tap(q => q.Marked("btnModal"));
				RunningApp.SetOrientationPortrait();
				RunningApp.Tap(q => q.Marked("btnDismissModal"));
				RunningApp.Tap("Main Page");
				RunningApp.Tap(q => q.Marked("btnMaster"));
				RunningApp.WaitForNoElement("btnMaster");
			}
			else
			{
				// Wait for the test to finish loading before exiting otherwise
				// the next UI test might start running while this is still loading
				RunningApp.WaitForElement(q => q.Marked("btnModal"));
			}
		}

		[TearDown]
		public override void TearDown()
		{
			RunningApp.SetOrientationPortrait();

			base.TearDown();
		}
#endif
	}
}
