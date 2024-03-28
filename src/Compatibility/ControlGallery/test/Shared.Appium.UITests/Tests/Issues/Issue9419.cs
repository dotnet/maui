using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9419 : IssuesUITest
	{
		const string OkResult = "Ok";

		public Issue9419(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash when toolbar item removed then page changed";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void TestIssue9419()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(OkResult);
		}
	}
}