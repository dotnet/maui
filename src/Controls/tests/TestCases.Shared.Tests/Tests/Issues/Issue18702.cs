#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18702 : _IssuesUITest
	{
		const string element = "collectionView";

		public Issue18702(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "The CollectionView has a grouped footer template that crashes the application";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void CollectionViewGroupedFooterTemplateShouldNotCrash()
		{
			App.WaitForElement(element);
			VerifyScreenshot();
		}
	}
}
#endif