using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19820 : _IssuesUITest
{
    public override string Issue => "Implicit styling is being ignored on the page level";

    public Issue19820(TestDevice device)
    : base(device)
    { }

    [Test]
    [Category(UITestCategories.Label)]
    public void ImplicitStylesCascadeFromApplicationToPageLevel()
    {
        App.WaitForElement("TestLabel");
        VerifyScreenshot();
    }
}