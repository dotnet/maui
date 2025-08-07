using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23377 : _IssuesUITest
{
    public Issue23377(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Horizontal Item spacing in collectionView";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void Issue23377ItemSpacing()
    {
        App.WaitForElement("ChangeItemSpace");
        App.Tap("ChangeItemSpace");
        VerifyScreenshot();
    }
}
