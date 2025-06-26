using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla45702 : _IssuesUITest
	{
		public Bugzilla45702(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabling back press on modal page causes app to crash";

		[Fact]
		[Trait("Category", UITestCategories.Navigation)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue45702Test()
		{
			App.WaitForElement("ClickMe");
			App.Tap("ClickMe");
			App.WaitForElement("Success");
		}
	}
}