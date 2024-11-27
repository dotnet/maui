﻿#if ANDROID || IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class SwipeViewUITests : UITest
	{
		const string ScrollViewGallery = "SwipeView Gallery";

		const string SwipeViewToRightId = "SwipeViewToRightId";
		const string ResultToRightId = "ResultToRightId";
		const string SwipeViewToLeftId = "SwipeViewToLeftId";
		const string ResultToLeftId = "ResultToLeftId";

		public SwipeViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(ScrollViewGallery);
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Description("Swipe to right the SwipeView")]
		public void SwipeToRight()
		{
			// 1. Open the SwipeView using a gesture.
			App.WaitForElement(SwipeViewToRightId);
			App.SwipeLeftToRight(SwipeViewToRightId);

			// 2. Check if the SwipeView has been opened correctly.
			var result = App.FindElement(ResultToRightId).GetText();
			ClassicAssert.AreEqual("Success", result);

			App.Screenshot("The SwipeView is Open");
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Description("Swipe to left the SwipeView")]
		public void SwipeToLeft()
		{
			// 1. Open the SwipeView using a gesture.
			App.WaitForElement(SwipeViewToLeftId);
			App.SwipeRightToLeft(SwipeViewToLeftId);

			// 2. Check if the SwipeView has been opened correctly.
			var result = App.FindElement(ResultToLeftId).GetText();
			ClassicAssert.AreEqual("Success", result);

			App.Screenshot("The SwipeView is Open");
		}
	}
}
#endif