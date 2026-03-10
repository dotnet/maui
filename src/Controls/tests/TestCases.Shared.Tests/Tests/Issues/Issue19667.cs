#if ANDROID || IOS // The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19667 : _IssuesUITest
{
	public Issue19667(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView contents not sizing correctly after orientation change";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewItemsSizeCorrectlyAfterOrientationChange()
	{
		App.TapShellFlyoutIcon();
		App.Tap("CollectionViewPage");
		App.WaitForElement("CollectionView19667");
		App.WaitForElement("CvItem0");

		var portraitWidth = App.WaitForElement("CvItem0").GetRect().Width;

		App.TapShellFlyoutIcon();
		App.Tap("Page1");
		App.WaitForElement("Page1Label");

		App.SetOrientationLandscape();

		App.TapShellFlyoutIcon();
		App.Tap("CollectionViewPage");
		App.WaitForElement("CollectionView19667");
		App.WaitForElement("CvItem0");

		var landscapeWidth = App.WaitForElement("CvItem0").GetRect().Width;
		Assert.That(landscapeWidth, Is.GreaterThan(portraitWidth),
			"CollectionView items should resize to landscape width after orientation change.");
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif
