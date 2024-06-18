using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2339 : IssuesUITest
	{
		public Issue2339(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Picker not shown when .Focus() is called";

		[Test]
		[Category(UITestCategories.Picker)]
		[FailsOnIOS]
		public void FocusAndUnFocusMultipleTimes()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows], "Focus Behavior is different");

			RunningApp.WaitForElement("btnFocusThenUnFocus");
			RunningApp.Tap("btnFocusThenUnFocus");
			RunningApp.WaitForNoElement("Picker Focused: 1");
			RunningApp.WaitForNoElement("Picker UnFocused: 1");
			RunningApp.Back();
			RunningApp.Tap("btnFocusThenUnFocus");
			RunningApp.WaitForNoElement("Picker Focused: 2");
			RunningApp.WaitForNoElement("Picker UnFocused: 2");
		}
	}
}