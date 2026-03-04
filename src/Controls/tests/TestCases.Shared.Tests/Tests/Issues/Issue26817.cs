#if MACCATALYST || IOS || ANDROID
using NUnit.Framework;
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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AccessibilityTraitsSetCorrectly()
		{
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "None");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Single");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Multiple");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "None");
		}
	}
}
#endif
