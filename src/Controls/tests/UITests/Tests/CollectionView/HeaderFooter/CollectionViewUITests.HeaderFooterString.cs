using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewHeaderFooterStringUITests : CollectionViewUITests
	{
		public CollectionViewHeaderFooterStringUITests(TestDevice device)
			: base(device)
		{
		}
				
		[Test]
		public void StringHeaderFooter()
		{
			App.ScrollTo("HeaderFooterString", true);
			App.Click("HeaderFooterString");

			// 1. Check CollectionView header and footer using a string.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}
	}
}