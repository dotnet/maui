#if MACCATALYST || WINDOWS // This test verifies that "the class defines a custom TitleBar for a ContentPage" that works on Desktop platforms only.

using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24489_2 : _IssuesUITest
	{
		public Issue24489_2(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TitleBar with NavigationBar";

		protected override bool ResetAfterEachTest => true;

		[Fact]
		[Trait("Category", UITestCategories.Window)]
		public void TitleBarWithSmallHeight()
		{
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest");
			VerifyScreenshot(GetCurrentTestName() + "_Initial");
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(GetCurrentTestName() + "_SmallHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenTitlebarWithSmallHeightRequest");
			VerifyScreenshot(GetCurrentTestName() + "_StartPageWithSmallTitleBar");
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(GetCurrentTestName() + "_SmallHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("SetTitleBarToNull").Tap();
			VerifyScreenshot(GetCurrentTestName() + "_Initial");

			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			VerifyScreenshot(GetCurrentTestName() + "_SmallHeightRequest");
		}

		[Fact]
		[Trait("Category", UITestCategories.Window)]
		public void TitleBarWithLargeHeight()
		{
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest");
			VerifyScreenshot(GetCurrentTestName() + "_Initial");
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			App.WaitForElement("WelcomeLabel").Tap(); // Move the cursor from the Back Button to avoid the cursor.
			VerifyScreenshot(GetCurrentTestName() + "_LargeHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenTitlebarWithLargeHeightRequest");
			VerifyScreenshot(GetCurrentTestName() + "_StartPageWithSmallTitleBar");
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			App.WaitForElement("WelcomeLabel").Tap(); // Move the cursor from the Back Button to avoid the cursor.
			VerifyScreenshot(GetCurrentTestName() + "_LargeHeightRequest");
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("SetTitleBarToNull").Tap();
			VerifyScreenshot(GetCurrentTestName() + "_Initial");

			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.WaitForElement("ToggleButton");
			App.WaitForElement("WelcomeLabel").Tap(); // Move the cursor from the Back Button to avoid the cursor.
			VerifyScreenshot(GetCurrentTestName() + "_LargeHeightRequest");
		}

		[Fact]
		[Trait("Category", UITestCategories.Window)]
		public void NavBarResetsColorAfterSmallTitleBar()
		{
			App.WaitForElement("OpenTitlebarWithSmallHeightRequest").Tap();
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenPageThatOpensEmptyTitleBar").Tap();
			VerifyScreenshot();
		}

		[Fact]
		[Trait("Category", UITestCategories.Window)]
		public void NavBarResetsColorAfterLargeTitleBar()
		{
			App.WaitForElement("OpenTitlebarWithLargeHeightRequest").Tap();
			App.TapBackArrow("Issue24489_2");

			App.WaitForElement("OpenPageThatOpensEmptyTitleBar").Tap();
			VerifyScreenshot();
		}
	}
}
#endif
