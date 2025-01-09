using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26066(TestDevice testDevice) : _IssuesUITest(testDevice)
{
	public override string Issue => "CollectionViewHandler2 RelativeSource binding to AncestorType not working";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void CollectionView2ShouldFindAncestorType()
    {
        var text = "CV2";
        var i = 1;
        var automationId = $"{text} - Item {i}".Replace(" ", "", StringComparison.Ordinal);
        App.WaitForElement(automationId);
        App.Click(automationId);
        App.WaitForElement("OK");
        App.Click("OK");
    }
}
