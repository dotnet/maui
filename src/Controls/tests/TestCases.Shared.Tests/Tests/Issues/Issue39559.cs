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

		try
		{
			App.Back();

			// Without the fix the callback registered on the Activity never runs while a modal is shown.
			App.WaitForTextToBePresentInElement("ModalBackPressCountLabel", "Modal count: 1");
			Assert.That(App.WaitForElement("ModalBackPressCountLabel").GetText(), Is.EqualTo("Modal count: 1"));
		}
		finally
		{
			// Without the fix Back already closed the modal, so only close it if it is still open.
			if (App.FindElements("CloseModalButton").Count > 0)
			{
				App.Tap("CloseModalButton");
			}
		}
	}
}
#endif
