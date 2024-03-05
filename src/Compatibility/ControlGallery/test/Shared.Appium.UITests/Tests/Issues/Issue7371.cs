using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
{
	public class Issue7371 : IssuesUITest
	{
		public Issue7371(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "iOS race condition(or not checking for null) of refreshing(offset animation) causes NullReferenceException";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public async Task RefreshingListViewCrashesWhenDisposedTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			await Task.Delay(500);
			RunningApp.WaitForElement("Success");
		}
	}
}