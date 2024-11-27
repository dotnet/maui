﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3884 : _IssuesUITest
	{
		public Issue3884(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "BoxView corner radius";

		[Test]
		[Category(UITestCategories.BoxView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue3884Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("I see a blue circle");
		}
	}
}