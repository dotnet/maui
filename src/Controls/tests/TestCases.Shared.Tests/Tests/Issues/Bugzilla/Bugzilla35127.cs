﻿using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.ScrollView)]
	public class Bugzilla35127UITests : _IssuesUITest
	{
		public Bugzilla35127UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "It is possible to craft a page such that it will never display on Windows";

		// Bugzilla35127 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla35127.cs)
		[Fact]
		public void Issue35127Test()
		{
			App.WaitForElement("See me?");
			var count = App.FindElements("scrollView").Count;
			Assert.True(count == 0);
			App.WaitForNoElement("Click Me?");
		}
	}
}