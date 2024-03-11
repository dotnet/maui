using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla44176 : IssuesUITest
	{
		public Bugzilla44176(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "InputTransparent fails if BackgroundColor not explicitly set on Android";

		[Test]
		[Category(UITestCategories.Layout)]
		public void Bugzilla44176Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("grid");
			RunningApp.Tap("grid");
			RunningApp.WaitForNoElement("Parent");

			RunningApp.WaitForElement("contentView");
			RunningApp.Tap("contentView");
			RunningApp.WaitForNoElement("Parent");

			RunningApp.WaitForElement("stackLayout");
			RunningApp.Tap("stackLayout");
			RunningApp.WaitForNoElement("Parent");

			RunningApp.WaitForElement("color");
			RunningApp.Tap("color");

			RunningApp.WaitForElement("grid");
			RunningApp.Tap("grid");
			RunningApp.WaitForNoElement("Parent");

			RunningApp.WaitForElement("contentView");
			RunningApp.Tap("contentView");
			RunningApp.WaitForNoElement("Parent");

			RunningApp.WaitForElement("stackLayout");
			RunningApp.Tap("stackLayout");
			RunningApp.WaitForNoElement("Parent");

			RunningApp.WaitForElement("nontransparent");
			RunningApp.Tap("nontransparent");

			RunningApp.WaitForElement("grid");
			RunningApp.Tap("grid");
			RunningApp.WaitForNoElement("Child");

			RunningApp.WaitForElement("contentView");
			RunningApp.Tap("contentView");
			RunningApp.WaitForNoElement("Child");

			RunningApp.WaitForElement("stackLayout");
			RunningApp.Tap("stackLayout");
			RunningApp.WaitForNoElement("Child");

			RunningApp.WaitForElement("color");
			RunningApp.Tap("color");

			RunningApp.WaitForElement("grid");
			RunningApp.Tap("grid");
			RunningApp.WaitForNoElement("Child");

			RunningApp.WaitForElement("contentView");
			RunningApp.Tap("contentView");
			RunningApp.WaitForNoElement("Child");

			RunningApp.WaitForElement("stackLayout");
			RunningApp.Tap("stackLayout");
			RunningApp.WaitForNoElement("Child");
		}
	}
}