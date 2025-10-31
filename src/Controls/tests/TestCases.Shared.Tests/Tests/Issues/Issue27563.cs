﻿#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
// On Catalyst, Swipe actions not supported in Appium.
// On Windows, TimeoutException is thrown when enabling the Loop. Refer issue: https://github.com/dotnet/maui/issues/29245
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27563 : _IssuesUITest
	{
		public override string Issue => "[Windows] CarouselView Scrolling Issue";

		public Issue27563(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewScrolling()
		{
			App.WaitForElement("carouselview");
			App.SwipeRightToLeft("carouselview");
			App.WaitForElement("Actual View");

			App.Tap("PositionButton");

			VerifyScreenshot();
		}
	}
}
#endif