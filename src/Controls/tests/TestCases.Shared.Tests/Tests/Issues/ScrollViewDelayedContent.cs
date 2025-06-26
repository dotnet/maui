﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.ScrollView)]
	public class ScrollViewDelayedContentUITests : _IssuesUITest
	{
		public ScrollViewDelayedContentUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Crash measuring empty ScrollView";

		// MeasuringEmptyScrollViewDoesNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue1538.cs)
		[Fact]
		public void MeasuringEmptyScrollViewDoesNotCrash()
		{
			// 1. Let's add a child after a second.
			// 2. Verify that the child has been added without exceptions.
			App.WaitForElementTillPageNavigationSettled("Foo");
		}
	}
}
