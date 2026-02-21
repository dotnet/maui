using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23797 : _IssuesUITest
{
    public Issue23797(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Binding context in ContentPresenter";

    [Test]
    [Category(UITestCategories.Layout)]
    public void ContentPresenterShouldPropagateBindingContextForTemplateBindings()
    {
        App.WaitForElement("Issue23797Btn");
        App.Tap("Issue23797Btn");
        var messageAfterClick = App.WaitForElement("CurrentMessageLabel").GetText();
        Assert.That(messageAfterClick, Is.EqualTo("success"));
    }
}