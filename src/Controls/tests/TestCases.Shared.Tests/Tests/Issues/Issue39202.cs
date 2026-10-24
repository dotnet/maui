
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
			var noShadow = App.WaitForElement("NoShadowLabel").GetRect();
			var shadow = App.WaitForElement("ShadowLabel").GetRect();
			Assert.That(shadow.X, Is.EqualTo(noShadow.X).Within(1),
				"Adding a shadow must not remove the item root's safe-area inset.");

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