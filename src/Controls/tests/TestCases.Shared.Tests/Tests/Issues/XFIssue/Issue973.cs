﻿#if TEST_FAILS_ON_ANDROID // IsPresented value is not reflected when change this on list view item tapped in flyout. Issue: https://github.com/dotnet/maui/issues/26324
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue973 : _IssuesUITest
{
	public Issue973(TestDevice testDevice) : base(testDevice)
	{
	}
#if ANDROID
	const string Tab1 = "TAB 1";
	const string Tab2 = "TAB 2";
#else
	const string Tab1 = "Tab 1";
	const string Tab2 = "Tab 2";
#endif
	public override string Issue => "ActionBar doesn't immediately update when nested TabbedPage is changed";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	[Description("Test tab reset when swapping out detail")]
	public void Issue973TestsTabResetAfterDetailSwap()
	{
		App.WaitForElement("Initial Page Left aligned");
		App.WaitForElement(Tab1);
		App.Tap(Tab2);
		App.WaitForElement("Initial Page Right aligned");
		App.Tap("Present Flyout");
		App.Tap("Page 4");
		App.WaitForElement("Close Flyout");
		App.Tap("Close Flyout");
		App.WaitForElement("Page 4 Left aligned");
		App.Tap(Tab2);
		App.WaitForElement("Page 4 Right aligned");
	}
}
#endif
