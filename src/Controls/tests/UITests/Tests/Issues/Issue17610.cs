using System.Drawing.Imaging;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
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
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS,
				TestDevice.Mac,
				TestDevice.Windows
			});

			if (App is not AppiumAndroidApp androidApp)
				throw new InvalidOperationException($"Invalid App Type For this Test: {App} Expected AppiumAndroidApp.");

			var rect1 = App.WaitForElement("Item4").GetRect();
			var rect2 = App.WaitForElement("Item16").GetRect();

			int fromX = rect1.CenterX();
			int fromY = rect1.CenterY();
			int toX = rect2.CenterX();
			int toY = rect2.CenterY();

			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
			var refreshSequence = new ActionSequence(touchDevice, 0);
			refreshSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, fromX, fromY, TimeSpan.Zero));
			refreshSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			refreshSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, toX, toY, TimeSpan.FromMilliseconds(250)));
			refreshSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, fromX, fromY, TimeSpan.FromMilliseconds(250)));
			refreshSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			androidApp.Driver.PerformActions([refreshSequence]);

			VerifyScreenshot();
		}
	}
}
