using System;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest.iOS;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7556, "[iOS] Masterbehavior.popover not being observed on iOS 13",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.FlyoutPage)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue7556 : TestFlyoutPage
	{
		protected override void Init()
		{
			Flyout = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label() { Margin = 20, Text = "Flyout Visible", TextColor = Colors.White }
					}
				},
				Title = "Flyout",
				BackgroundColor = Colors.Blue
			};

			Detail = new NavigationPage(new DetailsPage(this) { Title = "Details" });
		}

		[Preserve(AllMembers = true)]
		public class DetailsPage : ContentPage
		{
			FlyoutPage MDP { get; }
			Label lblThings;

			public DetailsPage(FlyoutPage FlyoutPage)
			{
				MDP = FlyoutPage;
				lblThings = new Label() { HorizontalTextAlignment = TextAlignment.Center, AutomationId = "CurrentMasterBehavior" };

				Content = new StackLayout()
				{
					Children =
					{
						lblThings,
						new Button()
						{
							Text = "Click to rotate through FlyoutLayoutBehavior settings and test each one",
							Command = new Command(OnChangeMasterBehavior),
							AutomationId = "ChangeMasterBehavior"
						},
						new Button()
						{
							Text = "Push Modal Page When on Split FlyoutLayoutBehavior",
							AutomationId = "PushModalPage",
							Command = new Command(() =>
							{
								Navigation.PushModalAsync(new ContentPage(){
									Content = new Button()
									{
										Text = "After popping this Page FlyoutLayoutBehavior should still be split",
										AutomationId = "PopModalPage",
										Command = new Command(() => Navigation.PopModalAsync())
									}
								});
							})
						},
						new Label(){ HorizontalTextAlignment = TextAlignment.Center, Text = "Close Flyout" }
					}
				};


				MDP.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
				lblThings.Text = MDP.FlyoutLayoutBehavior.ToString();
			}

			void OnChangeMasterBehavior()
			{
				var behavior = MDP.FlyoutLayoutBehavior;
				var results = Enum.GetValues(typeof(FlyoutLayoutBehavior)).Cast<FlyoutLayoutBehavior>().ToList();

				int nextIndex = results.IndexOf(behavior) + 1;
				if (nextIndex >= results.Count)
					nextIndex = 0;

				MDP.FlyoutLayoutBehavior = results[nextIndex];
				lblThings.Text = MDP.FlyoutLayoutBehavior.ToString();
			}
		}

#if UITEST && __IOS__
		[Test]
		public void MasterStillVisibleAfterPushingAndPoppingModalPage()
		{
			if (!RunningApp.IsTablet())
				return;

			RunningApp.SetOrientationLandscape();
			RunningApp.WaitForElement("Split");
			RunningApp.WaitForElement("Flyout Visible");
			RunningApp.Tap("PushModalPage");
			RunningApp.Tap("PopModalPage");
			RunningApp.WaitForElement("Flyout Visible");
		}

		[Test]
		public void SplitOnLandscapeFailsToDetectClose()
		{
			if (!RunningApp.IsTablet())
				return;

			while(RunningApp.WaitForElement("CurrentMasterBehavior")[0].ReadText() != FlyoutLayoutBehavior.SplitOnLandscape.ToString())
			{
				RunningApp.Tap("ChangeMasterBehavior");

				if(RunningApp.Query("Flyout Visible").Length > 0)
					RunningApp.Tap("Close Flyout");
			}

			RunningApp.Tap("Flyout");
			RunningApp.WaitForElement("Flyout Visible");
			RunningApp.Tap("Close Flyout");

			RunningApp.SetOrientationLandscape();
			RunningApp.SetOrientationPortrait();
			RunningApp.SetOrientationLandscape();
			RunningApp.SetOrientationPortrait();

			if (RunningApp.Query("Flyout Visible").Length > 0)
				RunningApp.Tap("Close Flyout");

			RunningApp.Tap("Flyout");
			RunningApp.WaitForElement("Flyout Visible");
			RunningApp.Tap("Close Flyout");
			RunningApp.Tap("Flyout");
			RunningApp.WaitForElement("Flyout Visible");
		}

		[TearDown]
		public override void TearDown() 
		{
			RunningApp.SetOrientationPortrait ();
			base.TearDown();
		}
#endif
	}
}
