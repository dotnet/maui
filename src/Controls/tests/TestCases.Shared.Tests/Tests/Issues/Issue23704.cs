#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Orientation change is not supported on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23704 : _IssuesUITest
	{
		public Issue23704(TestDevice testDevice) : base(testDevice)
		{
		}
		public override string Issue => "CollectionView items disappear after device rotation";

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
		public async Task CollectionViewItemsRemainVisibleAfterRotation()
		{
			App.WaitForElement("label");
			App.SetOrientationLandscape();
			await Task.Delay(1000); /// Wait for the orientation change. The Android scroll bar appears randomly, so wait for it to hide.
			VerifyScreenshot();
		}

		[TearDown]
		public void TearDown()
		{
    		App.SetOrientationPortrait();
		}
	}
}
#endif
