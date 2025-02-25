using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26066(TestDevice testDevice) : _IssuesUITest(testDevice)
{
	const string CV2Item1 = "CV2-Item1";

	public override string Issue => "CollectionViewHandler2 RelativeSource binding to AncestorType not working";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionView2ShouldFindAncestorType()
	{
		App.WaitForElement(CV2Item1);
		App.Click(CV2Item1);
		App.TapDisplayAlertButton("OK");
		App.WaitForElement(CV2Item1);
	}
}
