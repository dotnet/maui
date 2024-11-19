﻿#if TEST_FAILS_ON_CATALYST // Getting an OpenQA.Selenium.InvalidSelectorException : XQueryError:6 - "invalid type" in Line: 23. Using timeout also not works.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9006 : _IssuesUITest
	{
		const string ClickMe = "ClickMe";
		const string FinalLabel = "FinalLabel";
		public Issue9006(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void ClickingOnTabToPopToRootDoesntBreakNavigation()
		{
			App.WaitForElement(ClickMe);
			App.Tap(ClickMe);
			App.WaitForElement(FinalLabel);	
			App.TapBackArrow("Back");
			App.TapBackArrow("Tab 1");
			App.WaitForElement(ClickMe);
			App.Tap(ClickMe);
			Assert.That(App.WaitForElement(FinalLabel)?.GetText(), Is.EqualTo("Success"));
		}
	}
}