#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17105 : _IssuesUITest
{
    public Issue17105(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Hide password hint which is showing when the entry is focused";

    [Test]
    [Category(UITestCategories.Entry)]
    public void HidePasswordHint()
    {
        App.WaitForElement("Entry");
        App.Tap("Entry");
        VerifyScreenshot();
    }
}
#endif