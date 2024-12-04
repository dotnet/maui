using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26083 : _IssuesUITest
	{
		public Issue26083(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "UI not updating GridItemsLayout when Span becomes 1";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ItemsWrapGridShouldUpdateBasedOnCollectionViewSize()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}