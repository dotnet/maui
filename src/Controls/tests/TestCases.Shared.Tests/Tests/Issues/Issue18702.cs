using Xunit;
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

		public override string Issue => "When the CollectionView has a group footer template it should not crash the application";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void CollectionViewGroupFooterTemplateShouldNotCrash()
		{
			App.WaitForElement(element);
			VerifyScreenshot();
		}
	}
}