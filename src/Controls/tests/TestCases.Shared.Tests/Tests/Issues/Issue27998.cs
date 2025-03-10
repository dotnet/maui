using NUnit.Framework;
using OpenQA.Selenium.Internal;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27998 : _IssuesUITest
	{
		private const string _itemsId = "ItemsId";
		private const string _lastItemId = "LastItemId";
		private const string _firstItemId = "FirstItemId";
		private const string _lastItemText = "INSIDE SCROLL 27";

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
			var firstItem = App.WaitForElement(_firstItemId);
			var lastItem = App.WaitForElement(_lastItemId);
			Assert.That(lastItem.IsDisplayed().Equals(false));

			App.ScrollDown(_itemsId);
			App.WaitForTextToBePresentInElement(_lastItemId, _lastItemText);

			Assert.That(lastItem.IsDisplayed().Equals(true));
#endif
		}

		private void TestOtherPlatforms()
		{
#if !WINDOWS
			var firstItem = App.WaitForElement(_firstItemId);

			App.ScrollDown(_itemsId);
			var lastItem = App.WaitForElement(_lastItemId);
			Assert.That(lastItem.IsDisplayed().Equals(true));
#endif
		}
	}
}