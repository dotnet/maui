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
				var changeBoundsButton = App.WaitForElement("ChangeBoundsButton");
				// Allow layout to settle before screenshot
				Task.Delay(300).Wait();
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original", tolerance: 2.0);

				changeBoundsButton.Click();

				WaitForAllElements();
				Task.Delay(300).Wait();
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownPortrait", tolerance: 2.0);

#if IOS || ANDROID
				App.SetOrientationLandscape();

				WaitForAllElements();
				// Allow orientation change to settle
				Task.Delay(500).Wait();
#if ANDROID
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape", cropLeft: 125, tolerance: 2.0);
#else
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape", tolerance: 2.0);
#endif

				changeBoundsButton.Click();
				WaitForAllElements();

				App.SetOrientationPortrait();
				WaitForAllElements();
				Task.Delay(500).Wait();
				// Cannot use the original screenshot as the black bar on bottom is not as dark after rotation
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original2", tolerance: 2.0);
			}
			finally
			{
				App.SetOrientationPortrait();
			}
#endif
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