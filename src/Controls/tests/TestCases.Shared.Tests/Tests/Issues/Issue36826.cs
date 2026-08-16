using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36826 : _IssuesUITest
{
	public override string Issue => "ScrollView SoftInput safe area should keep bottom content above the keyboard";

	public Issue36826 /* NUnit provides the selected test device. */ (TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void BottomMarkerShouldScrollAboveSoftwareKeyboard()
	{
		if (!string.Equals(
			Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
			"36826",
			StringComparison.Ordinal))
		{
			return;
		}

		App.WaitForElement("SetSoftInputButton");
		App.Tap("SetSoftInputButton");
		App.WaitForTextToBePresentInElement("ModeLabel", "READY: SoftInput safe area enabled");

		App.ScrollDown("ReproScrollView", ScrollStrategy.Gesture, 0.75);
		App.ScrollDown("ReproScrollView", ScrollStrategy.Gesture, 0.75);
		App.ScrollDown("ReproScrollView", ScrollStrategy.Gesture, 0.75);
		App.WaitForElement("ReproEntry");
		App.Tap("ReproEntry");
		App.EnterText("ReproEntry", "keyboard visible");
		App.WaitForTextToBePresentInElement("ResultLabel", "READY: Swipe toward bottom, then evaluate");

		App.ScrollDown("ReproScrollView", ScrollStrategy.Gesture, 0.75);
		App.Tap("EvaluateButton");

		var result = App.FindElement("ResultLabel").GetText();
		Assert.That(result, Is.EqualTo("PASS: Bottom marker can scroll above keyboard"),
			"Bottom marker should scroll above the software keyboard when SoftInput safe area is enabled.");
	}
}
