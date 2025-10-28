#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// https://github.com/dotnet/maui/issues/26148
// In Windows, the foreground color is not applied to the custom icon, so as of now, the test is not applicable for Windows.
// To fix the issue on iOS anc Catalyst , separate PR https://github.com/dotnet/maui/pull/26016
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25920 : _IssuesUITest
	{
		public Issue25920(TestDevice device) : base(device) { }

		public override string Issue => ".NET MAUI set AppShell custom FlyoutIcon display problem";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ForegroundColorShouldbeSetandCustomIconAlignedProperly()
		{
			App.WaitForElement("Label");

			//The test passes on Android if the foreground color is applied to the icon
			VerifyScreenshot();
		}
	}
}
#endif