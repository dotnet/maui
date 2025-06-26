﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla31395 : _IssuesUITest
	{
		public Bugzilla31395(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash when switching MainPage and using a Custom Render";

		[Fact]
		[Trait("Category", UITestCategories.Navigation)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Bugzilla31395Test()
		{
			App.WaitForElement("SwitchMainPage");
			Assert.DoesNotThrow(() =>
			{
				App.Tap("SwitchMainPage");
			});
			App.WaitForElement("Hello");
		}
	}
}
