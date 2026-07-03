using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25534 : _IssuesUITest
	{
		public Issue25534(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Updating IconImageSource in ToolbarItem multiple times causes exception and crash after navigating back and forth between Shell pages";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemIconShouldNotCrashOnPageNavigation()
		{
			App.WaitForElement("GoToSecondPage");
			App.Tap("GoToSecondPage");
			App.WaitForElement("SecondPageLabel");
#if IOS || MACCATALYST
			App.WaitForElement("HomePage");
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				App.TapBackArrow(); // In iOS 26, the previous page title is not shown along with the back arrow, so we use the default back arrow
			}
			else
			{
				App.TapBackArrow("HomePage");
			}
#endif
#if ANDROID || WINDOWS
			App.TapBackArrow();
#endif
			App.WaitForElement("GoToSecondPage");
			App.Tap("GoToSecondPage");
			App.WaitForElement("SecondPageLabel");
		}
	}

}
