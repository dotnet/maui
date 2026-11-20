using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2951 : _IssuesUITest
{
	public Issue2951(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "On Android, button background is not updated when color changes ";

	[Test]
	[Category(UITestCategories.Button)]
	public void Issue2951Test()
	{
		App.WaitForElement("Ready");

		var initialButtonCount = App.FindElements("btnChangeStatus").Count();
		Assert.That(initialButtonCount, Is.EqualTo(3));

		App.FindElements("btnChangeStatus").ElementAt(1).Click();

		// The cell re-renders after the tap, which can invalidate element references
		// (StaleElementReferenceException on Mac Catalyst), so re-query on each attempt.
		App.RetryAssert(() =>
			Assert.That(App.FindElements("btnChangeStatus").ElementAt(1).GetText(), Is.EqualTo("B")));

		App.FindElements("btnChangeStatus").ElementAt(1).Click();
		App.RetryAssert(() =>
			Assert.That(App.FindElements("btnChangeStatus").Count(), Is.EqualTo(2)));

		var newSecondButton = App.FindElements("btnChangeStatus").ElementAt(1);
		newSecondButton.Click();

		// Use VerifyScreenshot to ensure the button background color has been updated properly
		// This screenshot is captured to visually confirm that the background color has changed as expected
		VerifyScreenshot();

	}
}