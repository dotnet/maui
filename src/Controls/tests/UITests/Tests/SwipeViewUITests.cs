using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
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
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			// 1. Open the SwipeView using a gesture.
			App.WaitForElement(SwipeViewToRightId);
			App.SwipeLeftToRight(SwipeViewToRightId);

			// 2. Check if the SwipeView has been opened correctly.
			var result = App.FindElement(ResultToRightId).GetText();
			Assert.AreEqual("Success", result);

			App.Screenshot("The SwipeView is Open");
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		[Description("Swipe to left the SwipeView")]
		public void SwipeToLeft()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			// 1. Open the SwipeView using a gesture.
			App.WaitForElement(SwipeViewToLeftId);
			App.SwipeRightToLeft(SwipeViewToLeftId);

			// 2. Check if the SwipeView has been opened correctly.
			var result = App.FindElement(ResultToLeftId).GetText();
			Assert.AreEqual("Success", result);

			App.Screenshot("The SwipeView is Open");
		}
	}
}