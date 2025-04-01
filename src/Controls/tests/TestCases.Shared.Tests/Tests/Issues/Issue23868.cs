#if TEST_FAILS_ON_CATALYST //Swipe actions cannot be performed on the macOS test server
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue23868 : _IssuesUITest
	{
		public Issue23868(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView with RefreshView Throws Exception During Pull-to-Refresh Actions";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewWithHeaderAndRefreshViewShouldNotCrashOnPullToRefresh()
		{
			App.WaitForElement("UpdateData");
			App.Tap("UpdateData");
			App.WaitForElement("CollectionView");
			App.ScrollUp("CollectionView", ScrollStrategy.Gesture, swipeSpeed: 7000);
			App.WaitForElement("CollectionView");
			App.ScrollUp("CollectionView", ScrollStrategy.Gesture, swipeSpeed: 7000);
			App.WaitForElement("CollectionView");
		}
	}
}
#endif