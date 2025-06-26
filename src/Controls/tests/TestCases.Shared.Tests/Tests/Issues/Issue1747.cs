using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1747 : _IssuesUITest
	{
		const string ToggleButtonAutomationId = nameof(ToggleButtonAutomationId);
		const string ToggleSwitchAutomationId = nameof(ToggleSwitchAutomationId);

		public Issue1747(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Binding to Switch.IsEnabled has no effect";

		[Fact]
		[Trait("Category", UITestCategories.Switch)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue1747Test()
		{
			App.WaitForElement(ToggleButtonAutomationId);
			App.WaitForElement(ToggleSwitchAutomationId);

			var toggleSwitch = App.FindElement(ToggleSwitchAutomationId);
			Assert.NotEqual(toggleSwitch, null);
			Assert.That(toggleSwitch?.IsEnabled(), Is.False);
			App.WaitForElement(ToggleButtonAutomationId);
			App.Tap(ToggleButtonAutomationId);

			toggleSwitch = App.FindElement(ToggleSwitchAutomationId);
			Assert.NotEqual(toggleSwitch, null);
			Assert.That(toggleSwitch?.IsEnabled(), Is.True);
		}
	}
}