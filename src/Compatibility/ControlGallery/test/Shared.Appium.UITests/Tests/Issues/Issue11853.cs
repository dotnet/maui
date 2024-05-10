#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue11853 : IssuesUITest
	{
		const string Run = "Run";

		public Issue11853(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug][iOS] Concurrent issue leading to crash in SemaphoreSlim.Release in ObservableItemsSource";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void JustWhalingAwayOnTheCollectionViewWithAddsAndClearsShouldNotCrash()
		{
			RunningApp.WaitForElement(Run);
			RunningApp.Tap(Run);
			Task.Delay(5000).Wait();
			RunningApp.Tap(Run);
			Task.Delay(5000).Wait();

			// If we can still find the button, then we didn't crash
			RunningApp.WaitForElement(Run);
		}
	}
}
#endif