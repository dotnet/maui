﻿#if ANDROID
using NUnit.Framework;
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

		// Issue13436 (src\ControlGallery\src\Issues.Shared\Issue13436.cs
		/*
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void ChangePeekAreaInsetsInOnSizeAllocatedTest()
		{
			App.WaitForElement("CarouselId");
		}
		*/
	}
}
#endif