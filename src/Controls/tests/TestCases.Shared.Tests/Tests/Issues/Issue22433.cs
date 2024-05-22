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
		public void ButtonLayoutAndSpacingTests()
		{
			App.WaitForElement("TestNavigateButtonLayout");
			VerifyScreenshot("Issue22433_Default");

			App.Tap("TestNavigateButtonLayout");
			App.WaitForElement("TestLayoutHeader");
			VerifyScreenshot("Issue22433_Layout");
			App.Back();

			App.WaitForElement("TestNavigateButtonPadding");
			App.Tap("TestNavigateButtonPadding");
			App.WaitForElement("TestSpacingHeader");
			VerifyScreenshot("Issue22433_Spacing");
		}
	}
}
