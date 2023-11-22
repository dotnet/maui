using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	class CollectionViewHeaderFooterTemplateUITests : CollectionViewUITests
	{
		public CollectionViewHeaderFooterTemplateUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void TemplateHeaderFooter()
		{
			App.ScrollTo("HeaderFooterTemplate", true);
			App.Click("HeaderFooterTemplate");

			// 1. Check CollectionView header and footer using a TemplatedView.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}
	}
}