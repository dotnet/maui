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
		public void ReappearingCollectionViewShouldNotThrowNRE()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.Click(FlyoutIconAutomationId);
			App.Click("CollectionView");
			App.Click("Toggle");
			App.Click("Toggle");
			App.Click(FlyoutIconAutomationId);
			App.Click("About");
			App.Click(FlyoutIconAutomationId);
			App.Click("CollectionView");
		}
	}
}