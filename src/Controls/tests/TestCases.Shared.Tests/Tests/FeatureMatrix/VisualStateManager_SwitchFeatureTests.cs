using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
public class VisualStateManager_SwitchFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerSwitchFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerSwitchFeatureTests;

	public VisualStateManager_SwitchFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Switch_InitialState()
	{
		App.WaitForElement("VSMSwitchButton");
		App.Tap("VSMSwitchButton");
		App.WaitForElement("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_Switch_On()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyVSM_Switch_Off()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_Switch_Reset()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
	}

	[Test, Order(7)]
	public void VerifyVSM_Switch_DisableWhileOn()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(12)]
	public void VerifyVSM_Switch_OnWhileDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(13)]
	public void VerifyVSM_Switch_OffWhileDisabled()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

}

