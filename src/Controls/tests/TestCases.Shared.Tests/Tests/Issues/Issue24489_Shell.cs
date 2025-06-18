#if MACCATALYST || WINDOWS // This test verifies that "the class defines a custom TitleBar for a ContentPage" that works on Desktop platforms only.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24489_Shell : _IssuesUITest
	{
		public Issue24489_Shell(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TitleBar with NavigationBar - Shell";

		protected override bool ResetAfterEachTest => true;

		[Test]
		[Category(UITestCategories.Window)]
		public void TitleBarWithSmallHeightShell()
		{
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Initial");
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_SmallHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenTitlebarWithSmallHeightRequest");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_StartPageWithSmallTitleBar");
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_SmallHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("SetTitleBarToNull").Tap();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Initial");

			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_SmallHeightRequest");
		}

		[Test]
		[Category(UITestCategories.Window)]
		public void TitleBarWithLargeHeightShell()
		{
			Exception? exception = null;

			App.WaitForElement("OpenTitlebarWithLargeHeightRequest");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Initial");
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			App.WaitForElement("WelcomeLabel").Tap(); // Move the cursor from the Back Button to avoid the cursor.
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_LargeHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenTitlebarWithLargeHeightRequest");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_StartPageWithSmallTitleBar");
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			App.WaitForElement("WelcomeLabel").Tap(); // Move the cursor from the Back Button to avoid the cursor.
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_LargeHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("SetTitleBarToNull").Tap();
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Initial");

			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			App.WaitForElement("WelcomeLabel").Tap(); // Move the cursor from the Back Button to avoid the cursor.
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_LargeHeightRequest");
		
			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Category(UITestCategories.Window)]
		public void NavBarResetsColorAfterSmallTitleBarShell()
		{
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenPageThatOpensEmptyTitleBar").Tap();
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Window)]
		public void NavBarResetsColorAfterLargeTitleBarShell()
		{
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenPageThatOpensEmptyTitleBar").Tap();
			VerifyScreenshot();
		}
	}
}
#endif
