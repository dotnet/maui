using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28415 : _IssuesUITest
	{
		public Issue28415(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView display is broken when setting IsVisible after items are added";

		[Test]
		[Category(UITestCategories.IsVisible)]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsShouldBeVisible()
		{
			App.WaitForElement("LoadListButton");
			App.Click("LoadListButton");
			App.WaitForElement("Item1");
		}

	}
}