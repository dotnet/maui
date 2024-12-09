﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla41153 : _IssuesUITest
{

#if ANDROID
	const string Tab1 = "TAB 1";
	const string Tab2 = "TAB 2";
	const string Tab3 = "TAB 3";
#else
	const string Tab1 = "Tab 1";
	const string Tab2 = "Tab 2";
	const string Tab3 = "Tab 3";
#endif
	public Bugzilla41153(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "jobject must not be IntPtr.Zero with TabbedPage and ToolbarItems";


	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Bugzilla41153Test()
	{
		App.WaitForElement("On Tab 1");
		App.Tap(Tab2);
		App.Tap(Tab3);
		App.WaitForElement("On Tab 3");
		App.Tap(Tab1);
		App.WaitForElement("On Tab 1");
		App.Tap("Toolbar Item");
		App.WaitForTextToBePresentInElement("Toolbar Item", "Success");
	}
}