using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue28163 : _IssuesUITest
{
	public Issue28163(TestDevice device) : base(device)
	{
	}

	public override string Issue => "RadioButton Focused and Unfocused Events Not Firing on iOS and Android";

    [Test]
    [Category(UITestCategories.RadioButton)]
    public void RadioButttonFocusEventsShouldFire()
    {
        App.WaitForElement("MauiRadioButton");

        App.Tap("MauiRadioButton");

        App.Tap("EmptyRadioButton");

        var focusedText = App.WaitForElement("FocusedLabel").GetText();

        var unfocusedText = App.WaitForElement("UnFocusedLabel").GetText();

        var checkChangedText = App.WaitForElement("CheckChangedLabel").GetText();

        Assert.That(focusedText,Is.EqualTo("True"));
        Assert.That(unfocusedText,Is.EqualTo("True"));
        Assert.That(checkChangedText,Is.EqualTo("True"));
    }
}
