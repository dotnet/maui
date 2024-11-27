﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16499 : _IssuesUITest
	{
		public Issue16499(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Crash when using NavigationPage.TitleView and Restarting App";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void AppDoesntCrashWhenReusingSameTitleView()
		{
			App.WaitForElement("SuccessLabel");
		}
	}
}
