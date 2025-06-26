﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22633 : _IssuesUITest
	{
		public Issue22633(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[iOS] Crash on when initializing a TabbedPage without children";

		[Fact]
		[Trait("Category", UITestCategories.TabbedPage)]
		public void ExceptionShouldNotBeThrown()
		{
			App.WaitForElement("label");
		}
	}
}
