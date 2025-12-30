#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS // Focus propagated for IOS and mac only. IOS does not have Tab key support.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28945 : _IssuesUITest
{
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
		
		// Send Tab key to trigger focus
		App.SendTabKey();

		// Verify that the content view is focused
		var focusText = App.WaitForElement(statusLabelId).GetText();
		Assert.That(focusText, Is.EqualTo(FocusSuccessMessage));

	}
}
#endif