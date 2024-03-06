using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue1747 : IssuesUITest
	{
		const string ToggleButtonAutomationId = nameof(ToggleButtonAutomationId);
		const string ToggleSwitchAutomationId = nameof(ToggleSwitchAutomationId);

		public Issue1747(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Binding to Switch.IsEnabled has no effect";

		[Test]
		[Category(UITestCategories.Switch)]
		public void Issue1747Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForElement(ToggleButtonAutomationId);
			RunningApp.WaitForElement(ToggleSwitchAutomationId);

			var toggleSwitch = RunningApp.FindElement(ToggleSwitchAutomationId);
			ClassicAssert.AreNotEqual(toggleSwitch, null);

			RunningApp.Tap(ToggleButtonAutomationId);

			toggleSwitch = RunningApp.FindElement(ToggleSwitchAutomationId);
			ClassicAssert.AreNotEqual(toggleSwitch, null);
		}
	}
}