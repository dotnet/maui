﻿#if TEST_FAILS_ON_CATALYST // This test fails on Catalyst because the animation happeds while tab changes which cause additional delays.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12642 : _IssuesUITest
	{
		public Issue12642(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Rapid ShellContent Navigation Causes Blank Screens";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void ClickingQuicklyBetweenTopTabsBreaksContent()
		{
			App.WaitForElement("AutomatedRun");
			App.Tap("AutomatedRun");
			App.WaitForElement("Success");
			App.Tap("AutomatedRun");
			App.WaitForElement("Success");		}
	}
}
#endif