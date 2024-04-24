﻿#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16321 : _IssuesUITest
	{
		public Issue16321(TestDevice device) : base(device) { }

		public override string Issue => "Alerts Open on top of current presented view";

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		public void OpenAlertWithModals()
		{
			App.WaitForElement("OpenAlertWithModals").Click();
			App.WaitForElement("Cancel").Click();
		}

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		public void OpenAlertWithNewUIWindow()
		{
			App.WaitForElement("OpenAlertWithNewUIWindow").Click();
			App.WaitForElement("Cancel").Click();
		}
	}
}
#endif