using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24996 : _IssuesUITest
	{
		public Issue24996(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Changing Translation of an element causes Maui in iOS to constantly run Measure & ArrangeChildren";

		[Test]
		[Category(UITestCategories.Layout)]
		public void ChangingTranslationShouldNotCauseLayoutPassOnAncestors()
		{
			App.WaitForElement("Stats");
			// Tries to translate the element in different positions, on-screen and off-screen.
			for (int i = 0; i < 4; i++)
			{
				App.Tap("Stats");
				// The app updates the "Stats" text asynchronously ~100ms after the tap.
				// Waiting only for the element to exist (WaitForElement) races with that
				// update and can read stale text, so wait for the expected text instead.
				bool textUpdated = App.WaitForTextToBePresentInElement("Stats", "Lvl1[0/0]");
				Assert.That(textUpdated, Is.True);
			}
		}
	}
}