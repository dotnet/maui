using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue5461 : IssuesUITest
	{
		const string Success = "If you can see this, the test has passed";

		public Issue5461(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ScrollView crashes when setting ScrollbarFadingEnabled to false in Custom Renderer";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewWithScrollbarFadingEnabledFalseDoesntCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(Success);
		}
	}
}
