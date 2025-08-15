#if MACCATALYST // Focus propagated for IOS and mac only.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28945 : _IssuesUITest
{
	const string toggleFocusButtonId = "TglFocusButton";
	const string statusLabelId = "statusLabel";
	const string FocusSuccessMessage = "ContentView Focused";
	const string UnfocusSuccessMessage = "ContentView UnFocused";

	public Issue28945(TestDevice testDevice) : base(testDevice)
	{

	}

	public override string Issue => "Add Focus propagation to MauiView";

	[Test]
	[Category(UITestCategories.Layout)]
	public void MauiViewShouldPropagateFocus()
	{
		App.WaitForElement("Issue28945_ContentView");
		if (App is not AppiumApp app)
		{
			return;
		}

		// https://developer.apple.com/documentation/xctest/xcuikeyboardkey?language=objc
		string[] keys = ["XCUIKeyboardKeyTab"]; // Tab Key

		app.Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
		{
			{ "keys", keys },
		});

		App.WaitForElement(toggleFocusButtonId);
		App.Tap(toggleFocusButtonId);

		// Verify that the content view is focused
		var focusText = App.WaitForElement(statusLabelId).GetText();
		Assert.That(focusText, Is.EqualTo(FocusSuccessMessage));

		App.WaitForElement(toggleFocusButtonId);
		App.Tap(toggleFocusButtonId);

		// Verify that the content view is unfocused
		var unfocusText = App.WaitForElement(statusLabelId).GetText();
		Assert.That(unfocusText, Is.EqualTo(UnfocusSuccessMessage));
	}
}
#endif