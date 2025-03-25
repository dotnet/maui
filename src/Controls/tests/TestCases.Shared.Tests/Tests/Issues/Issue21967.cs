using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21967 : _IssuesUITest
	{
		public Issue21967(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView causes invalid measurements on resize";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsResizeWhenContraintsOnCollectionViewChange()
		{
			var largestSize = App.WaitForElement("Item1").GetRect();
			App.Tap("Resize");
			var mediumSize = App.WaitForElement("Item1").GetRect();
			App.Tap("Resize");
			var smallSize = App.WaitForElement("Item1").GetRect();

			ClassicAssert.Greater(largestSize.Width, mediumSize.Width);
			ClassicAssert.Greater(mediumSize.Width, smallSize.Width);
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewFirstItemCorrectlySetsTheMeasure()
		{
			var itemSize = App.WaitForElement("Item1").GetRect();
			ClassicAssert.Greater(200, itemSize.Height);
		}
#if IOS || ANDROID //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public async Task CollectionViewWorksWhenRotatingDevice()
		{
			try
			{
				App.WaitForElement("FullSize");
				App.Tap("FullSize");
				App.SetOrientationPortrait();
				await Task.Delay(300);
				var itemSizePortrait = App.WaitForElement("Item1").GetRect();
				App.SetOrientationLandscape();
				await Task.Delay(300);
				var itemSizeLandscape = App.WaitForElement("Item1").GetRect();
				App.SetOrientationPortrait();
				await Task.Delay(300);
				var itemSizePortrait2 = App.WaitForElement("Item1").GetRect();

				ClassicAssert.Greater(itemSizeLandscape.Width, itemSizePortrait.Width);
				ClassicAssert.AreEqual(itemSizePortrait2.Width, itemSizePortrait.Width);
			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
#endif
	}
}