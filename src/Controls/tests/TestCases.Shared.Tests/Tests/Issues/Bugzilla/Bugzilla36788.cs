#if IOS
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36788 : _IssuesUITest
	{
		public Bugzilla36788(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Truncation Issues with Relative Layouts";

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		[Trait("Category", UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Bugzilla36788Test()
		{
			App.WaitForNoElement("Passed");
		}
	}
}
#endif