using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8715 : _IssuesUITest
	{
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
			App.WaitForElement("8715 About");
			App.TapShellFlyoutIcon();
			App.WaitForElement("CollectionView");
			App.Tap("CollectionView");
			App.Tap("Toggle");
			App.WaitForElement("Toggle");
			App.Tap("Toggle");
			App.TapShellFlyoutIcon();
			App.Tap("About");
			App.WaitForElement("8715 About");
			App.TapShellFlyoutIcon();
			App.WaitForElement("CollectionView");
			App.Tap("CollectionView");
		}
	}
}