using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22433 : _IssuesUITest
	{
		public Issue22433(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Button Device Tests Default";

		[Test]
		public async Task ButtonLayoutAndSpacingTests()
		{
			App.WaitForElement("TestNavigateButtonLayout");
			VerifyScreenshot("Issue22433_Default");

			App.Tap("TestNavigateButtonLayout");
			App.WaitForElement("TestLayoutHeader");
			await Task.Delay(500); // wait for nav animation
			VerifyScreenshot("Issue22433_Layout");

			App.WaitForElement("NavBackButton");
			App.Tap("NavBackButton");

			App.WaitForElement("TestNavigateButtonPadding");
			App.Tap("TestNavigateButtonPadding");
			App.WaitForElement("TestSpacingHeader");
			await Task.Delay(500);  // wait for nav animation
			VerifyScreenshot("Issue22433_Spacing");
		}
	}
}
