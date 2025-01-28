﻿#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6258 : _IssuesUITest
{
	public Issue6258(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ContextActions icon not working";

	[Test]
	[Category(UITestCategories.ListView)]
	public void ContextActionsIconImageSource()
	{
		App.WaitForElement("ListViewItem");
		App.ActivateContextMenu("ListViewItem");
		VerifyScreenshot();
	}
}
#endif