using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue5765 : IssuesUITest
	{
		const string Target = "FirstLabel";

		public Issue5765(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Frame, CollectionView, Android]The Label.Text is invisible on Android if DataTemplate have frame as layout";
	
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void FlexLayoutsInFramesShouldSizeCorrectly()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			// If the first label is visible at all, then this has succeeded
			App.WaitForElement(Target);
		}
	}
}