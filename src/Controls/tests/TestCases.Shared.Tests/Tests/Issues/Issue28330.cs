using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28330 : _IssuesUITest
{
    public Issue28330(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Stepper allows to increment when value equals to maximum";

    [Test]
    [Category(UITestCategories.Stepper)]
    public void Issue28330StepperIncrementShouldBeDisabled()
    {
        App.WaitForElement("stepperValue");
        App.IncreaseStepper("stepper");
        Assert.That(App.FindElement("stepperValue").GetText(), Is.EqualTo("1"));
    }
}