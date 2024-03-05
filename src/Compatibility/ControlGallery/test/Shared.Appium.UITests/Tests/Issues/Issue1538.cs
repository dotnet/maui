using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1538 : IssuesUITest
	{
		public Issue1538(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash measuring empty ScrollView"; 
		
		[Test]
		[Category(UITestCategories.ScrollView)]
		public void MeasuringEmptyScrollViewDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			Task.Delay(1000).Wait();
			RunningApp.WaitForElement("Foo");
		}
	}
}