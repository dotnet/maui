using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1469 : IssuesUITest
	{
		const string Go = "Select 3rd item";
		const string Success = "Success";

		public Issue1469(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting SelectedItem to null inside ItemSelected event handler does not work";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[FailsOnIOS]
		public void Issue1469Test()
		{
			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);
			RunningApp.WaitForElement(Success);
		}
	}
}