
#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue39202 : _IssuesUITest
	{
		public override string Issue => "SafeAreaEdges on a CollectionView item template root is ignored when the root has a Shadow";

		public Issue39202(TestDevice device) : base(device) { }

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
		public void CollectionViewItemSafeAreaEdgesShouldBePreservedWithShadow()
		{
			App.SetOrientationLandscape();
			App.WaitForElement("NoShadowLabel");
			App.WaitForElement("ShadowLabel");

			VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
		}

		[TearDown]
		public void TearDown()
		{
			App.SetOrientationPortrait();
		}
	}
}
#endif