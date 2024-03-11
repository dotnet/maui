using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla59896 : IssuesUITest
	{
		const string BtnAdd = "btnAdd";

		public Bugzilla59896(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "v2.4.0: Adding inserting section to ListView causes crash IF first section is empty ";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla59896Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(BtnAdd);
			RunningApp.Tap(BtnAdd);
		}
	}
}