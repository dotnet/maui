using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18071 : _IssuesUITest
	{
		public Issue18071(TestDevice device) : base(device) { }

		public override string Issue => "Windows and Android no longer draw Borders/clipping correctly";

		[Test]
		public void Issue18071Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"Currently fails on Android and Windows; see https://github.com/dotnet/maui/issues/17125");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
