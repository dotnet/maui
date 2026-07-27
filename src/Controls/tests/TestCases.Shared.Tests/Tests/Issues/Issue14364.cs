using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14364 : _IssuesUITest
{
    public Issue14364(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Control size properties are not available during Loaded event";

    [Test]
    [Category(UITestCategories.Layout)]
    public void SizePropertiesAvailableDuringLoadedEvent()
    {
        var label = App.WaitForElement("labelSize");
        Assert.That(label.GetText(), Is.Not.EqualTo("Height: -1, Width: -1"));
    }
}