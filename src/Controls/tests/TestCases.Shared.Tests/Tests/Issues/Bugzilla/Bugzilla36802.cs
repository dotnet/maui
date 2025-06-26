#if TEST_FAILS_ON_WINDOWS
// While using this GroupShortNameBinding property it throws an exception on Windows
// for more information:https://github.com/dotnet/maui/issues/26534. 
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36802 : _IssuesUITest
	{
		public Bugzilla36802(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] AccessoryView Partially Hidden When Using RecycleElement and GroupShortName";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Bugzilla36802Test()
		{
			App.WaitForElement("TestReady");
			VerifyScreenshot();
		}
	}
}
#endif