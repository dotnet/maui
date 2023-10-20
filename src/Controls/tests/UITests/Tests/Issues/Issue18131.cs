using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18131 : _IssuesUITest
	{
		public Issue18131(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Color changes are not reflected in the Rectangle shapes";

		[Test]
		public void Issue15330Test()
		{
			App.WaitForElement("ChangeColorButton");
			App.Click("ChangeColorButton");
			VerifyScreenshot();
		}
	}
}