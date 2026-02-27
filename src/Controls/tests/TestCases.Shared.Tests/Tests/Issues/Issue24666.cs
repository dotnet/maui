using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24666 : _IssuesUITest
	{
		public Issue24666(TestDevice device) : base(device) { }

		public override string Issue => "Differentiate Left/Right click in PointerGestureRecognizer";

		int ReadCount(string automationId)
		{
			var text = App.WaitForElement(automationId).GetText(); // e.g. "Events: 3"
			if (text is null)
				return 0;
			var parts = text.Split(':');
			return parts.Length == 2 && int.TryParse(parts[1].Trim(), out var n) ? n : 0;
		}

		void TapCenterOf(string automationId)
		{
			var r = App.WaitForElement(automationId).GetRect();
			App.TapCoordinates(r.CenterX(), r.CenterY());
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void PrimaryTapIncrementsPrimaryAndReleases()
		{
			// Use the counter label as the anchor; border itself isn't exposed on Windows UIA
			var before = ReadCount("PrimaryEventCount");

			TapCenterOf("PrimaryEventCount");

			var after = ReadCount("PrimaryEventCount");
			Assert.That(after, Is.EqualTo(before + 1));

			// PointerReleased should be logged for primary
			App.WaitForTextToBePresentInElement("EventLog", "Primary Button Released", timeout: System.TimeSpan.FromSeconds(5));
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void SecondaryAreaDoesNotReactToPrimaryTap()
		{
			var before = ReadCount("SecondaryEventCount");

			TapCenterOf("SecondaryEventCount");

			var after = ReadCount("SecondaryEventCount");
			// A primary tap should not trigger a Secondary-only recognizer
			Assert.That(after, Is.EqualTo(before));
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void CombinedAreaIncrementsOnPrimaryTapAndReleases()
		{
			var before = ReadCount("CombinedEventCount");

			TapCenterOf("CombinedEventCount");

			var after = ReadCount("CombinedEventCount");
			Assert.That(after, Is.EqualTo(before + 1));

			// PointerReleased should be logged for combined recognizer
			App.WaitForTextToBePresentInElement("EventLog", "Combined Button Released (Button: Primary)", timeout: System.TimeSpan.FromSeconds(5));
		}
	}
}
