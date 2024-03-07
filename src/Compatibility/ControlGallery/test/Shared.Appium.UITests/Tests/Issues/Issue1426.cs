using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1426 : IssuesUITest
	{
		public Issue1426(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SetHasNavigationBar screen height wrong";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void Github1426Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Screenshot("You can see the coffe mug");
			RunningApp.WaitForElement("CoffeeImageId");
			RunningApp.WaitForElement("NextButtonID");
			RunningApp.Tap("NextButtonID");
			RunningApp.WaitForElement("PopButtonId");
			RunningApp.Tap("PopButtonId"));
			RunningApp.WaitForElement("CoffeeImageId");
			RunningApp.Screenshot("Coffe mug Image is still there on the bottom");
		}
	}
}