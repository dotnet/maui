using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33038 : _IssuesUITest
{
	public Issue33038(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Layout breaks on first navigation until soft keyboard appears/disappears";

	[Test]
	[Category(UITestCategories.Shell)]
	public void LayoutShouldBeCorrectOnFirstNavigation()
	{
		App.WaitForElement("StartPageLabel");
		App.Tap("GoToSignInButton");
		var beforeKeyboard = App.WaitForElement("SignInLabel").GetRect();

		App.Tap("EmailEntry");
		App.DismissKeyboard();
		var afterKeyboard = App.WaitForElement("SignInLabel").GetRect();

		Assert.That(beforeKeyboard.Y, Is.EqualTo(afterKeyboard.Y).Within(5));
	}
}