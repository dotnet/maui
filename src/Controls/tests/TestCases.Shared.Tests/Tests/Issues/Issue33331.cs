using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33331 : _IssuesUITest
{
    public override string Issue => "[Android] Picker IsOpen not reset when picker is dismissed";

    public Issue33331(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Picker)]
    public void PickerCanBeOpenedProgrammatically()
    {
        App.WaitForElement("TestPicker");
        App.WaitForElement("OpenPickerButton");
        App.WaitForElement("IsOpenLabel");

        var initialLabel = App.FindElement("IsOpenLabel").GetText();
        Assert.That(initialLabel, Is.EqualTo("IsOpen: False"));

        App.Tap("OpenPickerButton");
        App.ClosePicker(windowsTapx: 50, windowsTapy: 50);
        App.WaitForElement("IsOpenLabel");
        var closedLabel = App.FindElement("IsOpenLabel").GetText();
        Assert.That(closedLabel, Is.EqualTo("IsOpen: False"));
    }
}