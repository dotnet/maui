#if MACCATALYST || IOS || ANDROID
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26817 : _IssuesUITest
	{
		public Issue26817(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionViewHandler2 assigns Accessibility Traits with SelectionMode correctly";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		public void AccessibilityTraitsSetCorrectly()
		{
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(GetCurrentTestName() + "None");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(GetCurrentTestName() + "Single");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(GetCurrentTestName() + "Multiple");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(GetCurrentTestName() + "None");
		}
	}
}
#endif
