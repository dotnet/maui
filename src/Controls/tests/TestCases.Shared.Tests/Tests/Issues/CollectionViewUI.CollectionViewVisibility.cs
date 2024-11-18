using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewVisibilityUITests : _IssuesUITest
	{
		const string Success = "Success";
		const string Show = "Show";

		public CollectionViewVisibilityUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "iOS application suspended at UICollectionViewFlowLayout.PrepareLayout() when using IsVisible = false";

		// InitiallyInvisbleCollectionViewSurvivesiOSLayoutNonsense(src\Compatibility\ControlGallery\src\Issues.Shared\Issue12714.cs)
#if ANDROID
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void InitiallyInvisbleCollectionViewSurvivesiOSLayoutNonsense()
		{
			App.WaitForElement(Show);
			App.Click(Show);
			App.WaitForNoElement(Success);
		}
#endif
	}
}