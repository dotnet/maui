using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewPreselectedItemUITests : CollectionViewUITests
	{
		public CollectionViewPreselectedItemUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("The preselected Item have a background color")]
		public void PreselectedItemCollectionView()
		{
			App.Click("PreselectedItem");
			App.WaitForElement("TestCollectionView");

			// 1. Check the preselected item.
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}