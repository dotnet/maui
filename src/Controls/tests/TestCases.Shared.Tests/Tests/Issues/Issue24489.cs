#if MACCATALYST || WINDOWS // This test verifies that "the class defines a custom TitleBar for a ContentPage" that works on Desktop platforms only
using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.Window)]
		public void TitleBarIsImplemented1()
		{
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(GetCurrentTestName() + "_Initial");
			App.WaitForElement("ToggleButton").Tap();
			VerifyScreenshot(GetCurrentTestName() + "_Removed");
			App.WaitForElement("ToggleButton").Tap();
			VerifyScreenshot(GetCurrentTestName() + "_Initial");
			App.WaitForElement("ToggleButton2").Tap();
			VerifyScreenshot(GetCurrentTestName() + "_TextAndSizeChanged");
		}
	}
}
#endif
