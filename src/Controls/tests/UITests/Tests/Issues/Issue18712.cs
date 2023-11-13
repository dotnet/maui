using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18712 : _IssuesUITest
	{
		public Issue18712(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Editor IsEnabled and IsVisible works";

		[Test]
		public void Issue18712Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}