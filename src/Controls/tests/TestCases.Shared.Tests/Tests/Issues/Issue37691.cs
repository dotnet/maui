#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37691 : _IssuesUITest
{
	public Issue37691(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Page scrolling behavior upon keyboard hide is broken";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void UnderlyingPageRestoresAfterDismissingModalWithKeyboardVisible()
	{
		App.WaitForElement("ShowModal");
		var initialBottom = App.WaitForElement("BottomMarker").GetRect().Bottom;
		App.Screenshot("Underlying page before showing modal");

		for (var cycle = 1; cycle <= 3; cycle++)
		{
			App.Tap("ShowModal");
			App.WaitForElement("ModalEntry");
			Assert.That(App.WaitForKeyboardToShow(), Is.True, "The keyboard must be visible before dismissing the modal.");

			if (cycle == 1)
				App.Screenshot("Modal with keyboard visible");

			App.Tap("DismissModal");
			App.WaitForElement("ShowModal");
			Assert.That(App.WaitForKeyboardToHide(), Is.True, "The keyboard must hide after dismissing the modal.");

			var restoredBottom = App.WaitForElement("BottomMarker").GetRect().Bottom;
			TestContext.Progress.WriteLine($"Cycle {cycle}: initial bottom={initialBottom}; restored bottom={restoredBottom}");
			Assert.That(restoredBottom, Is.EqualTo(initialBottom).Within(2),
				$"The underlying page did not restore its original height after cycle {cycle}. Initial bottom: {initialBottom}; restored bottom: {restoredBottom}.");
		}

		App.Screenshot("Underlying page after dismissing modal");
	}
}
#endif
