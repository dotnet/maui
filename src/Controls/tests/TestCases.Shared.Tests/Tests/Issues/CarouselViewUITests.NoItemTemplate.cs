﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewNoItemTemplate : _IssuesUITest
	{
		public CarouselViewNoItemTemplate(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "[Bug] CarouselView NRE if item template is not specified";

		[Fact]
		[Trait("Category", UITestCategories.CarouselView)]
		public void Issue12777Test()
		{
			App.WaitForElement("TestCarouselView");
			App.Screenshot("Test passed");
		}
	}
}
