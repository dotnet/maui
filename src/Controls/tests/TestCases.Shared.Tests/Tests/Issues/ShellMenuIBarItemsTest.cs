using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
#if WINDOWS
	public class ShellMenuIBarItemsTest : _IssuesUITest
	{
		public ShellMenuIBarItemsTest(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shell MenuBarItems Test";

		[Test]
		[Category(UITestCategories.Shell)]
		public void SettingMenuBarItemColorsWork()
		{
			Task.Delay(millisecondsDelay: 100).Wait();

			VerifyScreenshot("VerifyMenuBarItemColorsWork");
		}
	}
#endif
}