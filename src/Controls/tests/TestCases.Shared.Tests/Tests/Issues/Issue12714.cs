using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12714 : _IssuesUITest
	{
		const string Success = "Success";
		const string Show = "Show";

		public Issue12714(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] iOS application suspended at UICollectionViewFlowLayout.PrepareLayout() when using IsVisible = false";

		[Test]
		[Category(UITestCategories.CollectionView)]

		public void InitiallyInvisbleCollectionViewSurvivesiOSLayoutNonsense()
		{
			App.WaitForElement(Show);
			App.Tap(Show);
			App.WaitForElement(Success);
		}
	}
}