#if WINDOWS //Menu bar items only for desktop apps. For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/menu-bar?view=net-maui-9.0                                                                                                                                                          
//On MacCatalyst, menu items are part of the native macOS menu bar (e.g., Apple, File, Edit) at the top of the screen. So this is not implemented for Mac
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
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
}
#endif