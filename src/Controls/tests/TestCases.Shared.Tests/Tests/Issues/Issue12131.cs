// On Windows, AutomationId is not working for RefreshView (layout) used in this test
// (https://github.com/dotnet/maui/issues/4715)
#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12131 : _IssuesUITest
	{
		public Issue12131(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "RefreshView - CollectionView sizing not working correctly inside VerticalStackLayout";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void RefreshViewHasNonZeroHeightInVerticalStackLayout()
		{
			App.WaitForElement("RefreshView12131");
			var rect = App.FindElement("RefreshView12131").GetRect();
			Assert.That(rect.Height, Is.GreaterThan(0), "RefreshView should have a non-zero height when placed inside a VerticalStackLayout");
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void CollectionViewDoesNotExceedAvailableWidth()
		{
			App.WaitForElement("CollectionView12131");
			var refreshRect = App.FindElement("RefreshView12131").GetRect();
			var collectionRect = App.FindElement("CollectionView12131").GetRect();
			Assert.That(collectionRect.Width, Is.LessThanOrEqualTo(refreshRect.Width + 1), "CollectionView width should not exceed RefreshView width");
		}
	}
}
#endif
