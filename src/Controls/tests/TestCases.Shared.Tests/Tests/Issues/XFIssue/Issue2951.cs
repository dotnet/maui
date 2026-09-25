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

		App.RetryAssert(() => Assert.That(FindStatusButtons().Count(), Is.EqualTo(3)));

		App.Tap("ChangeStatusB");

		App.RetryAssert(() => Assert.That(
			FindStatusButtons().Select(button => button.GetText()),
			Is.EqualTo(new[] { "A", "B", "A" })));

		App.Tap("ChangeStatusB");
		App.RetryAssert(() => Assert.That(FindStatusButtons().Count(), Is.EqualTo(2)));
		App.WaitForNoElement("ChangeStatusB");

		App.Tap("ChangeStatusC");
		App.RetryAssert(() => Assert.That(
			FindStatusButtons().Select(button => button.GetText()),
			Is.EqualTo(new[] { "A", "B" })));

		// Use VerifyScreenshot to ensure the button background color has been updated properly
		// This screenshot is captured to visually confirm that the background color has changed as expected
		VerifyScreenshot();

	}

	IEnumerable<IUIElement> FindStatusButtons() =>
		new[] { "ChangeStatusA", "ChangeStatusB", "ChangeStatusC" }
			.SelectMany(id => App.FindElements(id));
}