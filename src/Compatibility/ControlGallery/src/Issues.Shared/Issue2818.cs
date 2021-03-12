using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2818, "Right-to-Left MasterDetail in Xamarin.Forms Hamburger icon issue", PlatformAffected.Android)]
	public class Issue2818 : TestFlyoutPage
	{

		protected override void Init()
		{
			FlowDirection = FlowDirection.RightToLeft;
			Flyout = new ContentPage
			{
				Title = "Flyout",
				BackgroundColor = Color.SkyBlue,
				IconImageSource = "menuIcon",
				Content = new StackLayout()
				{
					Children =
					{
						new Button()
						{
							Text = "If you can see me the test has passed",
							AutomationId = "CloseMasterView",
							Command = new Command(() => IsPresented = false)
						}
					},
					AutomationId = "MasterLayout"
				},
				Padding = new Thickness(0, 42, 0, 0)
			};

			Detail = new NavigationPage(new ContentPage
			{
				Title = "Detail",
				Content = new StackLayout
				{
					Children = {
						new Label
						{
							Text = "The page must be with RightToLeft FlowDirection. Hamburger icon in main page must be going to right side. There should be visible text inside the Flyout View"
						},
						new Button
						{
							Text = "Set RightToLeft",
							Command = new Command(() => FlowDirection = FlowDirection.RightToLeft),
							AutomationId = "ShowRightToLeft"
						},
						new Button
						{
							Text = "Set LeftToRight",
							Command = new Command(() => FlowDirection = FlowDirection.LeftToRight),
							AutomationId = "ShowLeftToRight"
						},
						new Button
						{
							Text = "Open Flyout View",
							Command = new Command(() => IsPresented = true),
							AutomationId = "OpenMasterView"
						},
						new Label()
						{
							Text = Device.Idiom.ToString(),
							AutomationId = "Idiom"
						}
					}
				}
			});
		}

#if UITEST
		[Test]
		public void MasterViewMovesAndContentIsVisible()
		{
			var idiom = RunningApp.WaitForElement("Idiom");

			// This behavior is currently broken on a phone device Issue 7270
			if (idiom[0].ReadText() != "Tablet")
				return;

			RunningApp.Tap("OpenMasterView");
			RunningApp.Tap("CloseMasterView");
			RunningApp.SetOrientationLandscape();
			RunningApp.Tap("OpenMasterView");
			var positionStart = RunningApp.WaitForElement("CloseMasterView");
			RunningApp.Tap("ShowLeftToRight");

			var results = RunningApp.QueryUntilPresent(() =>
			{
				var secondPosition = RunningApp.Query("CloseMasterView");

				if (secondPosition.Length == 0)
					return null;

				if (secondPosition[0].Rect.X < positionStart[0].Rect.X)
					return secondPosition;

				return null;
			});

			Assert.IsNotNull(results, "Flyout View Did not change flow direction correctly");
			Assert.AreEqual(1, results.Length, "Flyout View Did not change flow direction correctly");

		}

#if __IOS__
		[Test]
		public void MasterViewSizeDoesntChangeAfterBackground()
		{
			var idiom = RunningApp.WaitForElement("Idiom");
			// This behavior is currently broken on a phone device Issue 7270
			if (idiom[0].ReadText() != "Tablet")
				return;

			RunningApp.SetOrientationLandscape();
			RunningApp.Tap("CloseMasterView");
			RunningApp.Tap("ShowLeftToRight");
			var windowSize = RunningApp.WaitForElement("MasterLayout")[0];
			RunningApp.SendAppToBackground(TimeSpan.FromSeconds(5));
			var newWindowSize = RunningApp.WaitForElement("MasterLayout")[0];
			Assert.AreEqual(newWindowSize.Rect.Width, windowSize.Rect.Width);
			Assert.AreEqual(newWindowSize.Rect.Height, windowSize.Rect.Height);

		}
#endif

#endif
	}
}
