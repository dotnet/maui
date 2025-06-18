#if TEST_FAILS_ON_WINDOWS //Test failing on windows for more information see https://github.com/dotnet/maui/issues/18481
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

		public override string Issue => "[Bug] CollectionView problems and crashes with IsGrouped is true";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void AddingItemsToGroupedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Success, timeout: TimeSpan.FromSeconds(5));
		}
	}
}
#endif