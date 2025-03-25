using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6077 : _IssuesUITest
{
	public Issue6077(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView (iOS) using horizontal grid does not display last column of uneven item count";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void LastColumnShouldBeVisible()
	{
		// If the partial column shows up, then Item 5 will be in it
		App.WaitForElement("Item 5");
	}
}