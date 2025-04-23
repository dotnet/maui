using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class NestedCollectionView : _IssuesUITest
{
	public NestedCollectionView(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Crash when creating a CollectionView inside a CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void NestedCollectionViewsShouldNotCrash()
	{
		// If this page loaded and didn't crash, we're good.
		App.WaitForElement("Success");
	}
}