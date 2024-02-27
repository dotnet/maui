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
		public void FocusAndUnFocusMultipleTimes()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows], "Focus Behavior is different");

			App.WaitForElement("btnFocusThenUnFocus");
			App.Click("btnFocusThenUnFocus");
			App.WaitForNoElement("Picker Focused: 1");
			App.WaitForNoElement("Picker UnFocused: 1");
			App.Click("btnFocusThenUnFocus");
			App.WaitForNoElement("Picker Focused: 2");
			App.WaitForNoElement("Picker UnFocused: 2");
		}
	}
}