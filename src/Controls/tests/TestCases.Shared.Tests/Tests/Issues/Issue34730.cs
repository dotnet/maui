#if IOS || ANDROID // Keyboard is only available on mobile platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34730 : _IssuesUITest
{
	public Issue34730(TestDevice device) : base(device) { }

	public override string Issue => "HideSoftInputOnTapped doesn't work on Modal Pages";

	[Test]
	[Category(UITestCategories.SoftInput)]
	public void HideSoftInputOnTappedWorksOnModalPage()
	{
		if (App.IsKeyboardShown())
		{
			App.DismissKeyboard();
		}

		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");

		try
		{
			App.WaitForElement("ModalEntry");
			App.Tap("ModalEntry");

			Assert.That(App.IsKeyboardShown(), Is.True);

			App.WaitForElement("ModalEmptySpace");
			App.Tap("ModalEmptySpace");

			Assert.That(App.IsKeyboardShown(), Is.False);
		}
		finally
		{
			App.DismissKeyboard();
			App.WaitForElement("CloseModalButton");
			App.Tap("CloseModalButton");
		}
	}
}
#endif
