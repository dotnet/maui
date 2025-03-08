#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28121 : _IssuesUITest
{

	public Issue28121(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Picker Focused/Unfocused Events Do Not Fire";

	[Test]
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Compatibility)]
	public void FocusAndUnfocusEventsShouldFire()
	{
		App.WaitForElement("picker");
		App.Tap("picker");
		App.WaitForElement("Item1");
		App.Click("Item1");
		var focusedLabelText = App.FindElement("focusedLabel").GetText();
		var unfocusedLabelText = App.FindElement("unfocusedLabel").GetText();

		Assert.That(focusedLabelText, Is.EqualTo("Focused: true"));
		Assert.That(unfocusedLabelText, Is.EqualTo("Unfocused: true"));
	}
}
#endif