#if !IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue15330 : _IssuesUITest
	{
		public Issue15330(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Grid wrong Row height";

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnMac("VerifyScreenshot method not implemented on macOS")]
		public void Issue15330Test()
		{
			// Currently fails on iOS; see https://github.com/dotnet/maui/issues/17125

			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
#endif
