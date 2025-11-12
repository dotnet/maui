#if WINDOWS  //Menu bar items only for desktop apps. For more information : https://learn.microsoft.com/en-us/dotnet/maui/user-interface/menu-bar?view=net-maui-9.0                                                                                                                                                          
//On MacCatalyst, menu items are part of the native macOS menu bar (e.g., Apple, File, Edit) at the top of the screen. So this is not implemented for Mac
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16119 : _IssuesUITest
{
	public override string Issue => "MenuFlyoutItem with FileImageSource displays icon in monochrome instead of original colors";

	public Issue16119(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Page)]
	public void VerifyMenuFlyoutIconDisplaysOriginalColor()
	{
		App.WaitForElement("Issue16119DescriptionLabel");
		App.Tap("Menu Flyout Item");

		VerifyScreenshot();
	}
}
#endif