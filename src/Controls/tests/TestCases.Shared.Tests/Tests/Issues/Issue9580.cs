using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9580 : _IssuesUITest
	{
		const string Success = "Success";
		const string Test9580 = "9580";

		public Issue9580(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView - iOS - Crash when adding first item to empty item group";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void AllEmptyGroupsShouldNotCrashOnItemInsert()
		{
			App.WaitForElement(Test9580);
			App.Tap(Test9580);
			App.WaitForElement(Success);
		}
	}
}