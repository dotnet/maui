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
