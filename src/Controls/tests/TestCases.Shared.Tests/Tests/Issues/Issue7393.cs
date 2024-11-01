using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7393 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue7393(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView problems and crashes with IsGrouped=\"true\"";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void AddingItemsToGroupedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Success, timeout: TimeSpan.FromSeconds(30));
		}
	}
}