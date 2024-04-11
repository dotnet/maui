using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue19509 : _IssuesUITest
	{
		public Issue19509(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Entry TextColor property not working when the Text value is bound after some time";

		[Test]
		public void Issue19509Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Click a button to update the text
			App.Click("button");

			// 2. Verify that the Entry TextColor is correct (Green).
			VerifyScreenshot();
		}
	}
}
