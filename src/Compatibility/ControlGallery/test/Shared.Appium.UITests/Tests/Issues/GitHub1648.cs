using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class GitHub1648 : IssuesUITest
	{
		public GitHub1648(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "FlyoutPage throws ArgumentOutOfRangeException";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void GitHub1648Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForElement("Reload");
			RunningApp.Tap("Reload");
			RunningApp.WaitForElement("Success");
		}
	}
}