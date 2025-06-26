#if TEST_FAILS_ON_WINDOWS //Background Color updates on Slider Track
//For more information : https://github.com/dotnet/maui/issues/25921
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla29128 : _IssuesUITest
	{
		public Bugzilla29128(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Slider background lays out wrong Android";

		[Fact]
		[Trait("Category", UITestCategories.LifeCycle)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Bugzilla29128Test()
		{
			App.WaitForElement("SliderId");
			VerifyScreenshot();
		}
	}
}
#endif