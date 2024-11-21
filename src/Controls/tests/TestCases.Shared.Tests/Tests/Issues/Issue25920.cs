#if !WINDOWS
// In Windows, the foreground color is not applied to the custom icon, 
// so as of now, the test is not applicable for Windows.
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

			//The test passes on Android if the foreground color is applied to the icon, and on iOS, the custom icon should be aligned to the left.
			VerifyScreenshot();
		}
	}
}
#endif