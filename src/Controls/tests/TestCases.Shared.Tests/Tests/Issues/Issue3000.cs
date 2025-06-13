using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3000 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue3000(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Horizontal ScrollView breaks scrolling when flowdirection is set to rtl";

		[Fact]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void RtlScrollViewStartsScrollToRight()
		{
			App.WaitForElement(Success);
		}
	}
}