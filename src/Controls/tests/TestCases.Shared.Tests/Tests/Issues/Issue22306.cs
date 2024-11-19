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
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original");

				changeBoundsButton.Click();

				WaitForAllElements();
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownPortrait");

#if IOS || ANDROID
				App.SetOrientationLandscape();

				WaitForAllElements();
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "SizeButtonsDownLandscape");
				changeBoundsButton.Click();
				WaitForAllElements();

				App.SetOrientationPortrait();
				WaitForAllElements();
				// Cannot use the original screenshot as the black bar on bottom is not as dark after rotation
				VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original2");
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
