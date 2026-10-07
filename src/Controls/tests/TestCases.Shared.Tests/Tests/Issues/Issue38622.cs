#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS    // The issue is reproduce only when rotating the device.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38622 : _IssuesUITest
{
	public override string Issue => "CollectionView layout changes after device rotation with RTL";

	public Issue38622(TestDevice device) : base(device) { }

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void CollectionViewRTLLayoutShouldRemainConsistentAfterRotation()
	{
		App.WaitForElement("TestCollectionView");

		App.SetOrientationLandscape();

		App.WaitForElement("TestCollectionView");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
