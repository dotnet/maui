﻿#if TEST_FAILS_ON_WINDOWS    //The images generated from the CI do not display the toolbar on the Windows platform. However, the icon color issue does not exist on Windows. Therefore, the test is restricted to exclude the Windows platform.
using Xunit;
using Xunit;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25912 : _IssuesUITest
	{
		public Issue25912(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ToolbarItem color when used with IconImageSource is always white";

		[Fact]
		[Trait("Category", UITestCategories.ToolbarItem)]
		public void VerifyToolbarItemIconColor()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}

	}
}
#endif