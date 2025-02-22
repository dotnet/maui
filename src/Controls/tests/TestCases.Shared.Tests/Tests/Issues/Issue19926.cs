using NUnit.Framework;
using UITest.Appium;
using UITest.Appium.AI;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19926 : _IssuesUITest
{
	public override string Issue => "[Android] Opacity bug on BoxView.Background";

	public Issue19926(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.BoxView)]
	public async Task PropertiesShouldBeCorrectlyApplied()
	{
		App.WaitForElement("button");
		App.Click("button");

		// A small delay to wait for the button ripple effect animation to complete.
		await Task.Delay(500);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.BoxView)]
	[Ignore("The necessary Azure OpenAI parameters are still to be added on CI")]
	public async Task PropertiesShouldBeCorrectlyAppliedWithAI()
	{
		App.WaitForElement("button");

		await App.TapWithAI("The Button to show a BoxView");

		// Find the element using a natural language prompt describing the Button.
		var button = await App.WaitForElementWithAI("The Button to show a BoxView");
		button?.Tap(); // Alternative: await App.TapWithAI("The Button to show a BoxView");

		App.WaitForElement("boxView2");

		// Use a prompt to verify something in the UI.
		bool correctColors = await App.VerifyWithAI("The colors used in the gradients are red and blue.");
		Assert.That(correctColors, Is.True);

		bool wrongColors = await App.VerifyWithAI("The colors used in the gradients are green and yellow.");
		Assert.That(wrongColors, Is.False);

		// Use AI to compare the App screenshot and determinate if the image is equal to a reference one.
		bool areEquals = await VerifyScreenshotWithAI("PropertiesShouldBeCorrectlyApplied");
		Assert.That(areEquals, Is.True);
	}
}