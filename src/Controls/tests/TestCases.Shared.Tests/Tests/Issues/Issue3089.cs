﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3089 : _IssuesUITest
	{
		const string Reload = "reload";
		const string Success = "success";

		public Issue3089(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TextCell text doesn't change when using Recycling on ListViews";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void ResettingItemsOnRecycledListViewKeepsOldText()
		{
			App.WaitForElement(Reload);
			App.Tap(Reload);
			App.WaitForElement(Success);
		}
	}
}