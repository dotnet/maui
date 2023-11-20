using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18896 : _IssuesUITest
	{
		public Issue18896(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Can scroll ListView inside RefreshView";

		[Test]
		public void Issue18896Test()
		{
			App.WaitForElement("WaitForStubControl");

			// The test passes if you are able to see the image, name, and location of each monkey.
			VerifyScreenshot();
		}
	}
}