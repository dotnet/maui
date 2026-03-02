#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID
// https://github.com/dotnet/maui/issues/26148
// In Windows, the foreground color is not applied to the custom icon, so as of now, the test is not applicable for Windows.
// https://github.com/dotnet/maui/pull/27502
// For Android, we have separate PR to fix the issue, so the test is not applicable for Android.
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

			VerifyScreenshot();
		}
	}
}
#endif