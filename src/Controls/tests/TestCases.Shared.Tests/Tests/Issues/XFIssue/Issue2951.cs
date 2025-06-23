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

		var buttonToClick = App.FindElements("btnChangeStatus").ElementAt(1);
		buttonToClick.Click();

		Assert.That(buttonToClick.GetText(), Is.EqualTo("B"));

		buttonToClick.Click();
		var updatedButtonCount = App.FindElements("btnChangeStatus").Count();
		Assert.That(updatedButtonCount, Is.EqualTo(2));

		buttonToClick = App.FindElements("btnChangeStatus").ElementAt(1);
		buttonToClick.Click();

		// Use VerifyScreenshot to ensure the button background color has been updated properly
		// This screenshot is captured to visually confirm that the background color has changed as expected
		VerifyScreenshot();

	}
}