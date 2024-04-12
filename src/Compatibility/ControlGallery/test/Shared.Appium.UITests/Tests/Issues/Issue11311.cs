using NUnit.Framework;

namespace UITests
{
	public class Issue11311 : IssuesUITest
	{
		public Issue11311(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Regression] CollectionView NSRangeException";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.TabbedPage)]
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			// If this hasn't already crashed, the test is passing
			RunningApp.FindElement("Success");
		}
	}
}