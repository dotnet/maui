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
			Exception? exception = null;

#if IOS || ANDROID
			try
			{
				App.SetOrientationPortrait();
#endif
				WaitForAllElements();
				var changeBoundsButton = App.WaitForElement("ChangeBoundsButton");
				// Use retryTimeout to allow layout to settle
				VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "Original", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

				changeBoundsButton.Click();

				WaitForAllElements();
				VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownPortrait", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

#if IOS || ANDROID
				App.SetOrientationLandscape();

				WaitForAllElements();
				// Use retryTimeout to allow orientation change to settle
#if ANDROID
				VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape", cropLeft: 125, tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#else
				VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif

				changeBoundsButton.Click();
				WaitForAllElements();

				App.SetOrientationPortrait();
				WaitForAllElements();
				// Cannot use the original screenshot as the black bar on bottom is not as dark after rotation
				// Android can consistently rerasterize the same post-rotation layout with minor antialiasing differences.
				var finalPortraitTolerance = _testDevice == TestDevice.Android ? 2 : 0.5;
				VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "Original2", tolerance: finalPortraitTolerance, retryTimeout: TimeSpan.FromSeconds(2));
			}
			finally
			{
				App.SetOrientationPortrait();
			}
#endif

			if (exception is not null)
			{
				throw exception;
			}
		}

		void WaitForAllElements()
		{
			App.WaitForElement("ButtonLeft");
			App.WaitForElement("ButtonTop");
			App.WaitForElement("ButtonRight");
			App.WaitForElement("ButtonBottom");
			App.WaitForElement("ChangeBoundsButton");
		}
	}
}