using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4981 : _IssuesUITest
{
	public Issue4981(TestDevice device) : base(device) { }

	public override string Issue => "DisplayPromptAsync respects initial value selection";

	[Test, Order(1)]
	[Category(UITestCategories.DisplayPrompt)]
	public void PromptInitialValueIsSelected()
	{
		VerifyPrompt("PreselectTrueButton");
	}

	[Test, Order(2)]
	[Category(UITestCategories.DisplayPrompt)]
	public void PromptInitialValueIsNotSelected()
	{
		VerifyPrompt("PreselectFalseButton");
	}

#if IOS
	[TestCase(true, "2")]
	[TestCase(false, "12342")]
	[Category(UITestCategories.DisplayPrompt)]
	public void PromptInitialValueSelectionIsAppliedOnSubmit(bool preselectInitialValue, string expectedResult)
	{
		var buttonId = preselectInitialValue ? "PreselectTrueButton" : "PreselectFalseButton";

		App.WaitForElement(buttonId);
		App.Tap(buttonId);
		var promptInput = App.WaitForElement(
			() => App.GetAlert()?.ByClass("XCUIElementTypeTextField").SingleOrDefault(),
			timeoutMessage: "Timed out waiting for the prompt text field.");
		Assert.That(App.WaitForKeyboardToShow(), Is.True, "The prompt keyboard should be shown initially.");

		// Typing replaces the initial value only when it is preselected.
		promptInput.SendKeys("2");

		// Dismissing the keyboard sends Return, which submits the prompt.
		App.DismissKeyboard();
		App.WaitForNoElement(() => App.GetAlert());

		var result = App.WaitForElement("PromptResult").GetText();
		Assert.That(result, Is.EqualTo(expectedResult));
	}
#endif

	void VerifyPrompt(string buttonId)
	{
		App.WaitForElement(buttonId);
		App.Tap(buttonId);
		App.WaitForElement(() => App.GetAlert());

		try
		{
			VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		}
		finally
		{
#if MACCATALYST
			var alert = App.WaitForElement(() => App.GetAlert());
			var okButton = alert.GetAlertButtons()
				.Single(button => button.GetText() == "OK");
			okButton.Click();
			App.WaitForNoElement(() => App.GetAlert());
#else
			App.TapDisplayAlertButton("OK");
#endif
		}
	}
}
