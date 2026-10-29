using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22306 : _IssuesUITest
	{
		public Issue22306(TestDevice device) : base(device) { }

		public override string Issue => "Resizing buttons' parents resolves layout";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonsLayoutResolveWhenParentSizeChanges()
		{
#if IOS || ANDROID
			try
			{
				App.SetOrientationPortrait();
#endif
				WaitForAllElements();
				// Use retryTimeout to allow layout to settle
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

				App.Tap("ChangeBoundsButton");

				WaitForAllElements(small: true);
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownPortrait", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

#if IOS || ANDROID
				App.SetOrientationLandscape();

				WaitForAllElements(small: true, landscape: true);
				// Use retryTimeout to allow orientation change to settle
#if ANDROID
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape", cropLeft: 125, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#else
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif

				App.Tap("ChangeBoundsButton");
				WaitForAllElements(landscape: true);

				App.SetOrientationPortrait();
				WaitForAllElements(landscape: false);
				// Cannot use the original screenshot as the black bar on bottom is not as dark after rotation
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original2", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
			}
			finally
			{
				App.SetOrientationPortrait();
			}
#endif
		}

		void WaitForAllElements(bool small = false, bool? landscape = null)
		{
			App.WaitForElement("ChangeBoundsButton");
			App.RetryAssert(() =>
			{
				var grid = App.WaitForElementAndGetRect("TopGrid");
				if (landscape.HasValue)
					Assert.That(grid.Width > grid.Height, Is.EqualTo(landscape.Value));

				foreach (var buttonId in new[] { "ButtonLeft", "ButtonTop", "ButtonRight", "ButtonBottom" })
				{
					var button = App.WaitForElementAndGetRect(buttonId);
					Assert.That(button.Height, Is.GreaterThan(0));
					Assert.That(button.Width, Is.EqualTo(grid.Width / (small ? 7.0 : 3.0)).Within(2));
				}
			});
		}
	}
}