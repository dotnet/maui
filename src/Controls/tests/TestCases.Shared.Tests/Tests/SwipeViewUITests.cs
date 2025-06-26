#if ANDROID || IOS
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class SwipeViewUITests : CoreGalleryBasePageTest
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

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ScrollViewGallery);
		}

		[Fact]
		[Trait("Category", UITestCategories.SwipeView)]
		public void SwipeToRight()
		{
			// 1. Open the SwipeView using a gesture.
			App.WaitForElement(SwipeViewToRightId);
			App.SwipeLeftToRight(SwipeViewToRightId);

			// 2. Check if the SwipeView has been opened correctly.
			var result = App.FindElement(ResultToRightId).GetText();
			Assert.Equal("Success", result);

			App.Screenshot("The SwipeView is Open");
		}

		[Fact]
		[Trait("Category", UITestCategories.SwipeView)]
		public void SwipeToLeft()
		{
			// 1. Open the SwipeView using a gesture.
			App.WaitForElement(SwipeViewToLeftId);
			App.SwipeRightToLeft(SwipeViewToLeftId);

			// 2. Check if the SwipeView has been opened correctly.
			var result = App.FindElement(ResultToLeftId).GetText();
			Assert.Equal("Success", result);

			App.Screenshot("The SwipeView is Open");
		}
	}
}
#endif