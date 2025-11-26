#if TEST_FAILS_ON_WINDOWS
//No Map control support in windows https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map?view=net-maui-9.0
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20612 : _IssuesUITest
{
	public Issue20612(TestDevice device) : base(device) { }

	public override string Issue => "Disconnecting Map Handler causes Map to crash on second page entrance and moving to region.";

	[Test]
	[Category(UITestCategories.Maps)]
	public void MapsShouldNotCrashWhenNavigationOccurs()
	{
		App.WaitForElement("MapButton");
		App.Tap("MapButton");
		App.WaitForElement("GoBackButton");
		App.Tap("GoBackButton");
		// second Navigation to Map page
		App.WaitForElement("MapButton");
		App.Tap("MapButton");
	}
}
#endif