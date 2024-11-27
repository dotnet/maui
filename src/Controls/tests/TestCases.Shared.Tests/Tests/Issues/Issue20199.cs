﻿#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20199 : _IssuesUITest
	{
		public override string Issue => "[iOS] Page titles do not appear until navigating when pushing a modal page at startup";

		public Issue20199(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Page)]
		[FailsOnMacWhenRunningOnXamarinUITest("VerifyScreenshot method not implemented on macOS")]
		public void TitleViewShouldBeVisible()
		{
			_ = App.WaitForElement("button");
			App.Click("button");

			// The test passes if the 'Home Page' title is visible
			VerifyScreenshot();
		}
	}
}
#endif