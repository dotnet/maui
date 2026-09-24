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

		App.RetryAssert(() => Assert.That(App.FindElements("btnChangeStatus").Count(), Is.EqualTo(3)));

		var buttonToClick = App.FindElements("btnChangeStatus").ElementAt(1);
		buttonToClick.Click();

		App.RetryAssert(() => Assert.That(
			App.FindElements("btnChangeStatus").Select(button => button.GetText()),
			Is.EqualTo(new[] { "A", "B", "A" })));

		var updatedButton = App.FindElements("btnChangeStatus").ElementAt(1);
		updatedButton.Click();
		App.RetryAssert(() => Assert.That(App.FindElements("btnChangeStatus").Count(), Is.EqualTo(2)));

		var newSecondButton = App.FindElements("btnChangeStatus").ElementAt(1);
		newSecondButton.Click();
		App.RetryAssert(() => Assert.That(
			App.FindElements("btnChangeStatus").Select(button => button.GetText()),
			Is.EqualTo(new[] { "A", "B" })));

		// Use VerifyScreenshot to ensure the button background color has been updated properly
		// This screenshot is captured to visually confirm that the background color has changed as expected
		VerifyScreenshot();

	}
}