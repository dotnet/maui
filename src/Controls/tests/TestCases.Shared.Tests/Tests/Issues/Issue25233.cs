#if TEST_FAILS_ON_CATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25233 : _IssuesUITest
	{
		public Issue25233(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView with SwipeView items behaves strangely";

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void OnlyManuallySwipedItemShouldBeOpened()
		{
			App.WaitForElement("Item1");
			App.SwipeLeftToRight("Item1");
			App.SwipeLeftToRight("Item2");
			App.SwipeLeftToRight("Item3");
			App.Click("button");

			App.WaitForElement("Item15");
			VerifyScreenshot();
		}
	}
}
#endif