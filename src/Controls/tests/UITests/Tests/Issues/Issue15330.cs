using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue15330 : _IssuesUITest
	{
		public Issue15330(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Grid wrong Row height";

		[Test]
		[Category(UITestCategories.Layout)]
		public void Issue15330Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/17125");

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
