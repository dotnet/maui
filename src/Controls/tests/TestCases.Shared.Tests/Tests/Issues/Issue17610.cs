#if ANDROID // This code is specifically for Android. It sets the background color of the refresh indicator 
// (MauiSwipeRefreshLayout) to red, ensuring enough contrast for screenshot comparison during testing. This is not applicable to other platforms.
using NUnit.Framework;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17610 : _IssuesUITest
	{
		public Issue17610(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Cancelling Refresh With Slow Scroll Leaves Refresh Icon Visible";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void RefreshIconDisappearsWhenUserCancelsRefreshByScrollingBackUp()
		{
			if (App is not AppiumAndroidApp androidApp)
				throw new InvalidOperationException($"Invalid App Type For this Test: {App} Expected AppiumAndroidApp.");

			var rect1 = App.WaitForElement("Item4").GetRect();
			var rect2 = App.WaitForElement("Item16").GetRect();

			int fromX = rect1.CenterX();
			int fromY = rect1.CenterY();
			int toX = rect2.CenterX();
			int toY = rect2.CenterY();

			DragCoordinatesAndRestore(androidApp, fromX, fromY, toX, toY);

			App.WaitForElement("RefreshStatusLabel");
			Assert.That(App.FindElement("RefreshStatusLabel").GetText(), Is.EqualTo("RefreshView Not Triggered"));
		}

		void DragCoordinatesAndRestore(AppiumAndroidApp androidApp, int fromX, int fromY, int toX, int toY)
		{
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var refreshSequence = new ActionSequence(touchDevice, 0);
			refreshSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, fromX, fromY, TimeSpan.Zero));
			refreshSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			refreshSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, toX, toY, TimeSpan.FromMilliseconds(250)));
			refreshSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, fromX, fromY, TimeSpan.FromMilliseconds(250)));
			refreshSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			androidApp.Driver.PerformActions([refreshSequence]);
		}
	}
}
#endif
