using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla38989 : IssuesUITest
	{
		const string Success = "If you can see this, the test passed.";

		public Bugzilla38989(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] NullReferenceException when using a custom ViewCellRenderer ";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla38989Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement(Success);
		}
	}
}