using NUnit.Framework;
using OpenQA.Selenium.Internal;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27998 : _IssuesUITest
	{
		const string ItemsId = "ItemsId";
		const string LastItemId = "LastItemId";
		const string FirstItemId = "FirstItemId";
		const string LastItemText = "INSIDE SCROLL 27";

		public Issue27998(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Windows] ScrollView is not scrolling to the bottom if in grid with *,auto Width";

		[Test]
		[Category(UITestCategories.Cells)]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Border)]
		public void GridAutosStarsScollToEndDisplaysLastItem()
		{
			TestWindows();
			TestOtherPlatforms();
		}

		private void TestWindows()
		{
#if WINDOWS
			var firstItem = App.WaitForElement(FirstItemId);
			var lastItem = App.WaitForElement(LastItemId);
			Assert.That(lastItem.IsDisplayed().Equals(false));

			App.ScrollDown(ItemsId);
			App.WaitForTextToBePresentInElement(LastItemId, LastItemText);

			Assert.That(lastItem.IsDisplayed().Equals(true));
#endif
		}

		private void TestOtherPlatforms()
		{
#if !WINDOWS
			var firstItem = App.WaitForElement(FirstItemId);

			App.ScrollDown(ItemsId);
			var lastItem = App.WaitForElement(LastItemId);
			Assert.That(lastItem.IsDisplayed().Equals(true));
#endif
		}
	}
}