#if !ANDROID
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla34061 : _IssuesUITest
	{
		public Bugzilla34061(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "RelativeLayout - First child added after page display does not appear";

		[Fact]
		[Ignore("The sample is crashing.")]
		[Trait("Category", UITestCategories.Layout)]
		[Trait("Category", UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest("The sample is crashing. More information: https://github.com/dotnet/maui/issues/21204")]
		public void Bugzilla34061Test()
		{
			App.Screenshot("I am at Bugzilla34061 ");
			App.WaitForElement("btnAdd");
			App.Tap("btnAdd");
			App.WaitForElement("Remove Me");
			App.Screenshot("I see the button");
		}
	}
}
#endif