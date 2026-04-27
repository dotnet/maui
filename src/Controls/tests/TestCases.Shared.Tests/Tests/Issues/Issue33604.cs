// iOS only: Android CI devices lack notch/cutout, making SafeArea screenshot verification ineffective.
#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33604 : _IssuesUITest
	{
		public override string Issue => "CollectionView does not respect content SafeAreaEdges choices";

		public Issue33604(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsShouldRespectSafeAreaEdges()
		{
			App.WaitForElement("TestCollectionView");
			App.SetOrientationLandscape();

			// Allow layout to settle after orientation change
			App.WaitForElement("TopLabel");

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
