#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39559 : _IssuesUITest
{
	public Issue39559(TestDevice device) : base(device) { }

	public override string Issue => "HandleOnBackPressed fails to trigger with modal open on Android";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ActivityBackCallbackFiresWhenModalIsOpen()
	{
		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");
		App.WaitForElement("ModalLabel");

		App.Back();

		// Without the fix the callback registered on the Activity never runs while a modal is shown.
		App.WaitForTextToBePresentInElement("ModalBackPressCountLabel", "Modal count: 1");
		Assert.That(App.FindElement("ModalBackPressCountLabel").GetText(), Is.EqualTo("Modal count: 1"));
	}
}
#endif
