using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35755 : _IssuesUITest
{
	public Issue35755(TestDevice device) : base(device)
	{
	}

	public override string Issue => "IndexOutOfBoundsException in RecalculateSpanPositions when a Label uses FormattedText, MaxLines, and TailTruncation";

	[Test]
	[Category(UITestCategories.Label)]
	public void FormattedTextWithMaxLinesAndTailTruncationShouldNotCrash()
	{
		App.WaitForElement("TriggerButton");
		App.Tap("TriggerButton");

		App.WaitForElement("ResultLabel");
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Success"), "Label should display truncated FormattedText without crashing.");
		App.WaitForElement("CrashTargetLabel");
	}
}
