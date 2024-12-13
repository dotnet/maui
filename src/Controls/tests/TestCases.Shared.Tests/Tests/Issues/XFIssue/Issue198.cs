﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue198 : _IssuesUITest
{
#if ANDROID
	const string PageOne = "PAGE ONE";
	const string PageThree = "PAGE THREE";
#else
	const string PageOne = "Page One";
	const string PageThree = "Page Three";
#endif
	public Issue198(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage shouldn't proxy content of NavigationPage";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Issue198TestsNREWithPopModal()
	{
		App.WaitForElement(PageOne);
		App.WaitForElement("Leave");
	
		App.Tap("Leave");

		// Due to the current architecture of the HostApp, we cannot navigate back to the Bug Repro's page.
		// Also it's not recommended to place a TabbedPage into a NavigationPage.
	    // App.WaitForElement("Bug Repro's");
		// App.EnterText("SearchBarGo", "G198");
		// App.Tap("SearchButton");
	
		App.WaitForElement(PageThree);
		App.Tap(PageThree);
	
		App.WaitForElement("No Crash");
	}
}