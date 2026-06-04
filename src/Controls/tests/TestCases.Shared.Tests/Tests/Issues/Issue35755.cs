using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35755 : _IssuesUITest
{
	public Issue35755(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] IndexOutOfBoundsException in RecalculateSpanPositions when Label uses FormattedText + MaxLines + TailTruncation";

	[Test]
	[Category(UITestCategories.Label)]
	public void FormattedTextWithMaxLinesAndTailTruncationShouldNotCrash()
	{
		// Wait for the page to load
		App.WaitForElement("TriggerButton");

		// Tap button — this assigns a multi-span FormattedString to a Label with
		// MaxLines=4 and TailTruncation three times in a row, which previously
		// caused a fatal IndexOutOfBoundsException on Android.
		App.Tap("TriggerButton");

		// If the app didn't crash, the result label will show "Success"
		App.WaitForElement("ResultLabel");
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Success"), "Label should display truncated FormattedText without crashing.");

		// Verify the label is visible and rendered
		App.WaitForElement("CrashTargetLabel");
	}
}
