using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8263 : _IssuesUITest
	{
		public Issue8263(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add On/Off VisualStates for Switch";

		[Fact]
		[Trait("Category", UITestCategories.Switch)]
		public void SwitchOnOffVisualStatesTest()
		{
			App.WaitForElement("Switch");
			App.Tap("Switch");
			VerifyScreenshot(GetCurrentTestName() + "_SwitchOff");
			App.WaitForElement("Switch");
			App.Tap("Switch");
			VerifyScreenshot(GetCurrentTestName() + "_SwitchOn");
		}
	}
}
