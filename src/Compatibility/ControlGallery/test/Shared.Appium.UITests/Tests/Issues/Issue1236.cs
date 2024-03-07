using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1236 : IssuesUITest
	{
		public Issue1236(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label binding";

		[Test]
		[Category(UITestCategories.Label)]
		public void DelayedLabelBindingShowsUp()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			Task.Delay(2000).Wait();
			RunningApp.WaitForNoElement("Lorem Ipsum Dolor Sit Amet");
		}
	}
}