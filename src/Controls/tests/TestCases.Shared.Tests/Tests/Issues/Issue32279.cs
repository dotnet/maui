using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32279 : _IssuesUITest
{
	public Issue32279(TestDevice device) : base(device) { }

	public override string Issue => "TapGestureRecognizer does not work on layouts without a Background on Windows";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnLayoutWithNoBackgroundShouldWork()
	{
		App.WaitForElement("TapTargetNoBackground");
		App.Tap("TapTargetNoBackground");
		var result = App.FindElement("ResultLabelNoBackground").GetText();
		Assert.That(result, Is.EqualTo("Tapped"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnLayoutWithBackgroundShouldWork()
	{
		App.WaitForElement("TapTargetWithBackground");
		App.Tap("TapTargetWithBackground");
		var result = App.FindElement("ResultLabelWithBackground").GetText();
		Assert.That(result, Is.EqualTo("Tapped"));
	}
}