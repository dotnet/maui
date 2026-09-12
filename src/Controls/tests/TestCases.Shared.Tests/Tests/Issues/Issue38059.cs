using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38059 : _IssuesUITest
{
    public Issue38059(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "CollectionView VerticalGrid has excessive spacing without an ItemTemplate";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void Issue38059_UntemplatedVerticalGridDisplaysCompactRowsInTwoColumns()
    {
        App.WaitForElement("InstructionsLabel");
        VerifyScreenshot();
    }
}
