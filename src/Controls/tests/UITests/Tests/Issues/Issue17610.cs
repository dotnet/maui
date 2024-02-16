using System.Drawing.Imaging;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
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

			var fromX = (int)rect1.CenterX();
			var fromY = (int)rect1.CenterY();
			var toX = (int)rect2.CenterX();
			var toY = (int)rect2.CenterY();

			new TouchAction(androidApp.Driver)
				.Press(fromX, fromY)
				.MoveTo(toX, toY)
				.MoveTo(fromX, fromY)
				.Release()
				.Perform();

			VerifyScreenshot();
		}
	}
}
