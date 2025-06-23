﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29458 : _IssuesUITest
{
	public Issue29458(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ScrollView content offset shifts unexpectedly when FlowDirection is set to RightToLeft";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewShouldWorkInRTL()
	{
		App.WaitForElement("RTLScrollView");
		for (int i = 0; i < 3; i++)
		{
			App.ScrollLeft("RTLScrollView");
			App.ScrollRight("LTRScrollView");
		}
		App.WaitForElement("Tab5RTL");
		App.WaitForElement("Tab5LTR");
	}
}