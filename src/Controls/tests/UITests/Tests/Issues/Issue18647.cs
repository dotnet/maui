using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue18647 : _IssuesUITest
	{
		public Issue18647(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Editor font properties";

		[Test]
		public void Issue18647Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
