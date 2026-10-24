#if MACCATALYST || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26817_2 : _IssuesUITest
	{
		public Issue26817_2(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionViewHandler assigns Accessibility Traits with SelectionMode correctly";

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
		public void AccessibilityTraitsSetCorrectly()
		{
			App.WaitForElement("ToggleSelectionModeButton").Click();
			AssertButtonTraits("None", false);
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "None");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			AssertButtonTraits("Single", true);
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Single");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			AssertButtonTraits("Multiple", true);
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Multiple");
			App.WaitForElement("ToggleSelectionModeButton").Click();
			AssertButtonTraits("None", false);
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "None");
		}

		void AssertButtonTraits(string selectionMode, bool isButton)
		{
			App.WaitForTextToBePresentInElement("SelectionModeStatus", $"selectionMode: {selectionMode}");
			App.WaitForTextToBePresentInElement("BorderButtonTrait", $"Border: Item is Button - {isButton}");
			App.WaitForTextToBePresentInElement("StackButtonTrait", $"VSL: Item is Button - {isButton}");
			App.WaitForTextToBePresentInElement("GridButtonTrait", $"Grid: Item is Button - {isButton}");
		}
	}
}
#endif
