﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1219 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue1219(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting ToolbarItems in ContentPage constructor crashes app";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void ViewCellInTableViewDoesNotCrash()
		{
			App.WaitForElement(Success);
		}
	}
}