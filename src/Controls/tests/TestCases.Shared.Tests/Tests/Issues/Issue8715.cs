#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8715 : _IssuesUITest
	{
		const string FlyoutIconAutomationId = "OK";

		public Issue8715(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NullReferenceException Microsoft.Maui.Controls.Platform.iOS.StructuredItemsViewRenderer [Bug]";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void ReappearingCollectionViewShouldNotThrowNRE()
		{
			App.Tap(FlyoutIconAutomationId);
			App.Tap("CollectionView");
			App.Tap("Toggle");
			App.Tap("Toggle");
			App.Tap(FlyoutIconAutomationId);
			App.Tap("About");
			App.Tap(FlyoutIconAutomationId);
			App.Tap("CollectionView");
		}
	}
}
#endif