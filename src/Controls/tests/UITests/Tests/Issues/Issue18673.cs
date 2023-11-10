using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue18673 : _IssuesUITest
	{
		public Issue18673(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Editor font properties";

		[Test]
		public void Issue18673Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
