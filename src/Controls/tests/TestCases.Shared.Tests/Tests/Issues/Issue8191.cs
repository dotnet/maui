#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8191 : _IssuesUITest
	{
		public override string Issue => "ContentButton item template roots apply CollectionView cell safe areas";

		public Issue8191(TestDevice device) : base(device) { }

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
		public void ContentButtonItemRootsMatchCellSafeAreaInset()
		{
			App.SetOrientationLandscape();
			var reference = App.WaitForElement("ReferenceLabel").GetRect();
			var direct = App.WaitForElement("ContentButtonLabel").GetRect();
			var wrapped = App.WaitForElement("ShadowContentButtonLabel").GetRect();

			// The reference Border root is inset past the landscape notch; ContentButton roots,
			// including one wrapped for its Shadow, must receive the same per-cell inset.
			Assert.That(reference.X, Is.GreaterThan(20));
			Assert.That(direct.X, Is.EqualTo(reference.X).Within(2));
			Assert.That(wrapped.X, Is.EqualTo(reference.X).Within(2));
		}

		[TearDown]
		public void TearDown()
		{
			App.SetOrientationPortrait();
		}
	}
}
#endif
