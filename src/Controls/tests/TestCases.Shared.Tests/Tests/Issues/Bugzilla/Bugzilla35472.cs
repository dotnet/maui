﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla35472 : _IssuesUITest
	{
		public Bugzilla35472(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PopAsync during ScrollToAsync throws NullReferenceException";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		[FailsOnMac]
		public void Issue35472PopAsyncDuringAnimatedScrollToAsync()
		{
			try
			{
				App.WaitForElement("PushButton");
				App.Tap("PushButton");

				App.WaitForElement("NowPushButton");
				App.Screenshot("On Page With ScrollView");
				App.Tap("NowPushButton");

				App.WaitForNoElement("The test has passed");
				App.Screenshot("Success");
			}
			finally
			{
				App.Back();
			}
		}
	}
}