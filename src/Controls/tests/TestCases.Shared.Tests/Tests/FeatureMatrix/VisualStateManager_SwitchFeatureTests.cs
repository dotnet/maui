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
	public void VerifyVSM_Switch_InitialState()
	{
		App.WaitForElement("VSMSwitchButton");
		App.Tap("VSMSwitchButton");
		App.WaitForElement("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot("Switch_Initial_State");
	}

	[Test, Order(2)]
	public void VerifyVSM_Switch_On()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		VerifyScreenshot("Switch_On_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_Switch_Off()
	{
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot("Switch_Off_State");
	}

	[Test, Order(4)]
	public void VerifyVSM_Switch_Reset()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Switch_Reset_State");
	}

	[Test, Order(5)]
	public void VerifyVSM_Switch_OnAndOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.Tap("VSMSwitch");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
	}

	[Test, Order(6)]
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
		VerifyScreenshot("Switch_On_And_Disable_State");
	}

	[Test, Order(7)]
	public void VerifyVSM_Switch_DisableWhileOff()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Switch_Off_And_Disable_State");
	}

	[Test, Order(8)]
	public void VerifyVSM_Switch_DisableAndReset()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();;
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Switch_Disable_And_Reset_State");
	}

	[Test, Order(9)]
	public void VerifyVSM_Switch_OnDisableAndReset()
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
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Switch_On_Disable_And_Reset_State");
	}

	[Test, Order(10)]
	public void VerifyVSM_Switch_OffDisableAndReset()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Switch_Off_Disable_And_Reset_State");
	}

	[Test, Order(11)]
	public void VerifyVSM_Switch_OnAfterDisable()
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

	[Test, Order(12)]
	public void VerifyVSM_Switch_OffAfterDisable()
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
	public void VerifyVSM_Switch_OnWhileDisableAndEnable()
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
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		VerifyScreenshot("Switch_On_While_Disable_And_Enable_State");
	}

	[Test, Order(14)]
	public void VerifyVSM_Switch_OffWhileDisableAndEnable()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Off"));
		VerifyScreenshot("Switch_Off_While_Disable_And_Enable_State");
	}

	[Test, Order(15)]
	public void VerifyVSM_Switch_ResetAfterDisable()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(16)]
	public void VerifyVSM_Switch_ResetAfterOnDisable()
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
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(17)]
	public void VerifyVSM_Switch_ResetAfterOffDisable()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("SwitchDisable");
		App.Tap("SwitchDisable");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(18)]
	public void VerifyVSM_Switch_OnAfterReset()
	{
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		App.WaitForElement("VSMSwitch");
		App.Tap("VSMSwitch");
		var stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: On"));
		App.WaitForElement("SwitchReset");
		App.Tap("SwitchReset");
		stateText = App.FindElement("SwitchState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}
}