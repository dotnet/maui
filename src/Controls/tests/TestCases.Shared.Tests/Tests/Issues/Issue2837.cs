using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2837 : _IssuesUITest
	{
		const string Success = "worked";

		public Issue2837(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Exception thrown during NavigationPage.Navigation.PopAsync";

		[Fact]
		[Trait("Category", UITestCategories.Navigation)]
		[Trait("Category", UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void Issue2837Test()
		{
			App.WaitForElement(Success);
		}
	}
}
