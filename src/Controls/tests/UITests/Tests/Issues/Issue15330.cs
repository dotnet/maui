using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue15330 : _IssuesUITest
	{
		public Issue15330(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Grid wrong Row height";

		[Test]
		public void Issue15330Test()
		{
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
