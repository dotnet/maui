using NUnit.Framework;
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

		[Test]
		[Category(UITestCategories.Switch)]
		public void SwitchOnOffVisualStatesTest()
		{
			App.WaitForElement("Switch");
			App.Tap("Switch");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_SwitchOff");
			App.WaitForElement("Switch");
			App.Tap("Switch");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_SwitchOn");
		}
	}
}
