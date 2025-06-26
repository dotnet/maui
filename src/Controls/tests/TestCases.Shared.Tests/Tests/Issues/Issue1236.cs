﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1236 : _IssuesUITest
	{
		public Issue1236(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label binding";

		[Fact]
		[Trait("Category", UITestCategories.Label)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void DelayedLabelBindingShowsUp()
		{
			Task.Delay(2000).Wait();
			App.WaitForElement("Lorem Ipsum Dolor Sit Amet");
		}
	}
}