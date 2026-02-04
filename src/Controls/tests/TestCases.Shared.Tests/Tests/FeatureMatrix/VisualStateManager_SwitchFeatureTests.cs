using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_SwitchFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerSwitchFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerSwitchFeatureTests;

	public VisualStateManager_SwitchFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void Switch_On_UpdatesStateLabel()
	{
		App.WaitForElement("VSMSwitchButton");
		App.Tap("VSMSwitchButton");
		App.WaitForElement("DemoSwitch");
		App.WaitForElement("SwitchState");
		App.Tap("DemoSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
	}

	[Test, Order(2)]
	public void Switch_Off_UpdatesStateLabel()
	{
		App.WaitForElement("DemoSwitch");
		App.Tap("DemoSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}

	[Test, Order(3)]
	public void Switch_Disable_UpdatesStateLabel()
	{
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(4)]
	public void Switch_Reset_ReturnsToOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}
}