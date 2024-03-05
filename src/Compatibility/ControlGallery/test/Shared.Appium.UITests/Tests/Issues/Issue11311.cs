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
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			// If this hasn't already crashed, the test is passing
			App.FindElement("Success");
		}
	}
}