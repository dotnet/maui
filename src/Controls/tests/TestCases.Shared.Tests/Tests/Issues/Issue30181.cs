using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30181 : _IssuesUITest
{
    public Issue30181(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "Entry bound to a double casts values too early, preventing small negative decimal entries";

    [Test]
    [Category(UITestCategories.Entry)]
    public void EntryBoundToDouble_AllowsTypingSmallNegativeDecimal()
    {
        App.WaitForElement("Issue30181Entry");
        App.EnterText("Issue30181Entry", "-0.01");
        var result = App.WaitForElement("Issue30181Entry").GetText();
        Assert.That(result, Is.EqualTo("-0.01"), "The Entry should allow typing a small negative decimal value.");
    }
}