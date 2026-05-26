using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30953 : _IssuesUITest
{
	public Issue30953(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "CollectionView does not update layout correctly when ItemsSource changes";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EnsureCollectionViewLayoutOnItemsSourceChange()
	{
		App.WaitForElement("Issue30953Button");
		App.Tap("Issue30953Button");
		App.WaitForElement("United States");
	}
}