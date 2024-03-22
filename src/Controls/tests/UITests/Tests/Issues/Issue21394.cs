using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21394 : _IssuesUITest
	{
		public Issue21394(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Buttons with Images layouts";

        [Test]
		public void Issue21394Test()
		{
			this.Ignore(TestDevice.iOS, () => {
                if (!OperatingSystem.IsIOSVersionAtLeast(15))
                    return true;
                return false;
            }, "iOS 14 and below sizes button images differently.");

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
