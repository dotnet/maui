using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32867: _IssuesUITest
{
 public Issue32867(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "Shell Flyout Icon is always black";
    
    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellFlyoutIconShouldNotBeBlack()
    {
        App.WaitForElement("Issue32867Label");
        VerifyScreenshot();
    }
}