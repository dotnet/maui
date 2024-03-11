using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla41205 : IssuesUITest
	{
		const string Success = "Pass";

		public Bugzilla41205(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "UWP CreateDefault passes string instead of object";

		[Test]
		[Category(UITestCategories.ListView)]
		public void CreateDefaultPassesStringInsteadOfObject()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForNoElement(Success);
		}
	}
}