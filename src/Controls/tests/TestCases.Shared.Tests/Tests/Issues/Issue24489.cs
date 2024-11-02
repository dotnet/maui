#if MACCATALYST || WINDOWS

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24489 : _IssuesUITest
	{
		public Issue24489(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TitleBar Implementation";

		[Test]
		[Category(UITestCategories.Window)]
        public void TitleBarIsImplemented1()
        {
            App.WaitForElement("ToggleButton");
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Initial");
            App.WaitForElement("ToggleButton").Tap();
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Removed");
            App.WaitForElement("ToggleButton").Tap();
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Initial");
        }
    }
}
#endif
