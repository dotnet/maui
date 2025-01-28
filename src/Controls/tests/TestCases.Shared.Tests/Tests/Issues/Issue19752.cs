using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19752 : _IssuesUITest
{
	public Issue19752(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Pointer-over visual state has higher priority than focused state";

	[Test]
	[Category(UITestCategories.Visual)]
	public async Task VerifyPointerOverStateHasHigherPrecedenceThanFocusedState()
	{
		// Wait until all elements are loaded.
		App.WaitForElement("Button2");

		// Tap on Button1 so that the click handler assigns makes the button focused.
		App.Click("Button1");

		await Task.Yield();
		VerifyScreenshot();

		ClassicAssert.True(App.IsFocused("Button1"));
	}
}