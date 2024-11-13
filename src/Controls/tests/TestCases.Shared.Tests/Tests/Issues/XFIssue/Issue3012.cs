using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3012 : _IssuesUITest
{
	public Issue3012(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Entry focus / unfocus behavior";

	[Test]
	[Category(UITestCategories.Entry)]
	public void Issue3012Test()
	{
		App.WaitForElement("DumbyEntry");
		App.Tap("DumbyEntry");

		App.WaitForElement("FocusTargetEntry");
		App.Tap("FocusTargetEntry");

		AssertUnfocusedCount(0, "Unfocused should not have fired");

		App.Tap("DumbyEntry");

		AssertUnfocusedCount(1, "Unfocused should have been fired once");
	}

	private void AssertUnfocusedCount(int expectedCount, string message)
	{
		App.WaitForElement("UnfocusedCountLabel");
		var unfocusedCountText = App.WaitForElement("UnfocusedCountLabel");
		var unfocusedCountText1 = unfocusedCountText.GetText();
		Assert.That(unfocusedCountText1, Is.EqualTo($"Unfocused count: {expectedCount}"), message);
	}
}
