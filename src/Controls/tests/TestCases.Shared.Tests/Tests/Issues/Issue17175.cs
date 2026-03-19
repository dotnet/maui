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

        // Normal → Disabled
        App.Tap("GoToDisabledButton");
        var stateLabel = App.FindElement("StateLabel").GetText();
        Assert.That(stateLabel, Is.EqualTo("State: Disabled"));

        // Disabled → Normal (verifies UnApply restores correctly)
        App.Tap("GoToDisabledButton");
        stateLabel = App.FindElement("StateLabel").GetText();
        Assert.That(stateLabel, Is.EqualTo("State: Normal"));

        // Normal → Disabled again (verifies second cycle works)
        App.Tap("GoToDisabledButton");
        stateLabel = App.FindElement("StateLabel").GetText();
        Assert.That(stateLabel, Is.EqualTo("State: Disabled"));
    }
}