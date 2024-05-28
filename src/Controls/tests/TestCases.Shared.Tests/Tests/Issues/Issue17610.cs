#if ANDROID
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

			App.DragCoordinates(fromX, fromY, toX, toY);

			VerifyScreenshot();
		}
	}
}
#endif
