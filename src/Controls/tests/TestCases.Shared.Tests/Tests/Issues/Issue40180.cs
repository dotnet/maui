using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40180 : _IssuesUITest
{
	public Issue40180(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Standalone long press should not fire after a quick tap";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void StandaloneLongPressDoesNotFireAfterQuickTap()
	{

		App.WaitForElement("LongPressTarget");
		App.WaitForElement("LongPressCountLabel");

		App.TapElement("LongPressTarget");

		// Check after the long-press timeout so a delayed callback cannot pass unnoticed.
		Assert.That(() => App.FindElement("LongPressCountLabel").GetText(),
			Is.EqualTo("Long Press Count: 0").After(1000),
			"A standalone long press should not fire after a quick tap");
	}
}
