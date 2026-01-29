using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17175 : _IssuesUITest
{
    public override string Issue => "VisualStateManager should be able to set Style property dynamically";

    public Issue17175(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Button)]
    public void VisualStateManagerCanSetStyleProperty()
    {
        App.WaitForElement("GoToDisabledButton");
        App.Tap("GoToDisabledButton");
        var stateLabel = App.FindElement("StateLabel").GetText();
        Assert.That(stateLabel, Is.EqualTo("State: Disabled"));
    }
}