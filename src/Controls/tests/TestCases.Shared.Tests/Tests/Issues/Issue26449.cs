using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26449 : _IssuesUITest
	{
		public Issue26449(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Unable to scroll inner CollectionView of nested CollectionViews";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void InnerCollectionViewOfNestedCollectionViewShouldScroll()
		{
			App.WaitForElement("CollectionView");
			App.ScrollDown("CollectionView", ScrollStrategy.Gesture);
			App.WaitForAnyElement(["Item5", "Item6", "Item7"]);
		}
	}
}