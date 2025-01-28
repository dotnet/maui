using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla59925 : _IssuesUITest
	{
		const string BiggerButton = "BiggerButton";
		const string TestEntry = "TestEntry";

		public Bugzilla59925(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Font size does not change vertical height of Entry on iOS";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla59925Test()
		{
			App.WaitForElement(BiggerButton);
			var initialSize = App.WaitForElement(TestEntry).GetRect().Height;

			// iOS/macOS Catalyst Workaround: Minimum Height Threshold
			// Issue: Entry control has a minimum vertical height on these platforms.
			// Impact: Font size increases don't affect height until exceeding this threshold.
			// Solution: Perform additional taps to ensure font size surpasses the minimum.
			// This allows subsequent size comparisons to accurately reflect height changes.
#if IOS || MACCATALYST
			for (int i = 0; i < 10; i++)
			{
				App.WaitForElement(BiggerButton);
				App.Tap(BiggerButton);
			}
#endif

			App.WaitForElement(BiggerButton);
			App.DoubleTap(BiggerButton);
			var updatedSize1 = App.WaitForElement(TestEntry).GetRect().Height;
			Assert.That(updatedSize1, Is.GreaterThanOrEqualTo(initialSize));

			App.WaitForElement(BiggerButton);
			App.DoubleTap(BiggerButton);
			var updatedSize2 = App.WaitForElement(TestEntry).GetRect().Height;
			Assert.That(updatedSize2, Is.GreaterThanOrEqualTo(updatedSize1));

			App.WaitForElement(BiggerButton);
			App.DoubleTap(BiggerButton);
			var updatedSize3 = App.WaitForElement(TestEntry).GetRect().Height;
			Assert.That(updatedSize3, Is.GreaterThanOrEqualTo(updatedSize2));
		}
	}
}