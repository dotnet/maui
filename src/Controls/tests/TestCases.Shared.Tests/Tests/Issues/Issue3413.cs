#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
//For more information : https://github.com/dotnet/maui/issues/27329
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3413 : _IssuesUITest
	{
		public Issue3413(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Searchbar in Horizontal Stacklayout doesn't render";

		[Fact]
		[Category(UITestCategories.SearchBar)]
		[Category(UITestCategories.Compatibility)]
		public void Issue3413Test()
		{
			App.WaitForElement("srb_vertical");
			VerifyScreenshot();
		}
	}
}
#endif