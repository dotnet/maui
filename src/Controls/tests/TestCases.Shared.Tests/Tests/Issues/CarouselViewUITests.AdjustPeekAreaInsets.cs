﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewAdjustPeekAreaInsets : _IssuesUITest
	{
		public CarouselViewAdjustPeekAreaInsets(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "[Bug] Java.Lang.IllegalArgumentException in CarouselView adjusting PeekAreaInsets in OnSizeAllocated using XF 5.0";

		[Fact]
		[Trait("Category", UITestCategories.CarouselView)]
		public void ChangePeekAreaInsetsInOnSizeAllocatedTest()
		{
			App.WaitForElement("CarouselId");
		}
	}
}