using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11853 : _IssuesUITest
	{
		const string Run = "Run";

		public Issue11853(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug][iOS] Concurrent issue leading to crash in SemaphoreSlim.Release in ObservableItemsSource";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		public void JustWhalingAwayOnTheCollectionViewWithAddsAndClearsShouldNotCrash()
		{
			App.WaitForElement(Run);
			App.Tap(Run);
			App.Tap(Run);

			// If we can still find the button, then we didn't crash
			App.WaitForElement(Run);
		}
	}
}