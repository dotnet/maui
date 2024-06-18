#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8715 : IssuesUITest
	{
		const string FlyoutIconAutomationId = "OK";

		public Issue8715(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NullReferenceException Microsoft.Maui.Controls.Platform.iOS.StructuredItemsViewRenderer [Bug]";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Shell)]
		public void ReappearingCollectionViewShouldNotThrowNRE()
		{
			RunningApp.Tap(FlyoutIconAutomationId);
			RunningApp.Tap("CollectionView");
			RunningApp.Tap("Toggle");
			RunningApp.Tap("Toggle");
			RunningApp.Tap(FlyoutIconAutomationId);
			RunningApp.Tap("About");
			RunningApp.Tap(FlyoutIconAutomationId);
			RunningApp.Tap("CollectionView");
		}
	}
}
#endif