#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID
// Windows: The snapshot does not capture the FlyoutIcon applied to the TitleBar.
// Android: The fix has not been implemented, so the icon defaults to white.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueShell6738 : _IssuesUITest
	{
		public override string Issue => "The color of the custom icon in Shell always resets to the default blue";

		public IssueShell6738(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void EnsureCustomFlyoutIconColor()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
#endif