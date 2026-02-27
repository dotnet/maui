using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.CheckBox)]
public class CheckBoxFeatureTests : _GalleryUITest
{
	const string IsCheckedLabel = "IsCheckedLabel";
	const string CheckBoxControl = "CheckBoxControl";
	const string ResetButton = "ResetButton";
	const string IsCheckedSwitch = "IsCheckedSwitch";
	const string IsEnabledSwitch = "IsEnabledSwitch";
	const string IsVisibleSwitch = "IsVisibleSwitch";
	const string BlueColorButton = "BlueColorButton";
	const string CommandStatusLabel = "CommandStatusLabel";
	const string CheckedChangedStatusLabel = "CheckedChangedStatusLabel";
	const string HasShadowCheckBox = "HasShadowCheckBox";

	public const string CheckBoxFeatureMatrix = "CheckBox Feature Matrix";

	public override string GalleryPageName => CheckBoxFeatureMatrix;

	public CheckBoxFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void CheckBox_ValidateDefaultValues_VerifyLabels()
	{
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
		App.WaitForElement(CheckBoxControl);
	}

	[Test, Order(2)]
	public void CheckBox_SetIsCheckedState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

	[Test, Order(3)]
	public void CheckBox_VerifyCheckedChangedEvent()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		App.WaitForNoElement("CheckedChanged Triggered");
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(CheckedChangedStatusLabel).GetText(), Is.EqualTo("CheckedChanged Triggered"));
	}

	[Test, Order(4)]
	public void CheckBox_SetVisibilityToFalse_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsVisibleSwitch);
		App.Tap(IsVisibleSwitch);
		Assert.That(App.FindElements(CheckBoxControl), Is.Empty);
		App.WaitForElement(IsVisibleSwitch);
		App.Tap(IsVisibleSwitch);
		Assert.That(App.FindElements(CheckBoxControl), Is.Not.Empty);
	}

	[Test, Order(5)]
	public void CheckBox_ChangeColor_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	public void CheckBox_SetIsCheckedAndColor_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	public void CheckBox_SetAllProperties_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

	[Test, Order(8)]
	public void CheckBox_VerifyCommandExecution()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed"));
	}

	[Test, Order(9)]
	public void CheckBox_VerifyCommandWithParameterWhileChecked()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement("CommandParameterEntry");
		App.EnterText("CommandParameterEntry", "TestParameter");
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed: TestParameter"));
	}

	[Test, Order(10)]
	public void CheckBox_VerifyCommandWithParameterWhileUnChecked()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement("CommandParameterEntry");
		App.EnterText("CommandParameterEntry", "TestParameter");
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed: TestParameter"));
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed: TestParameter"));
	}

	[Test, Order(11)]
	public void CheckBox_VerifyCommandNotExecutedWhenDisabled()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		App.WaitForNoElement("Command Executed");
	}

	[Test, Order(12)]
	public void CheckBox_VerifyBothEventAndCommandExecuted()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(CheckedChangedStatusLabel).GetText(), Is.EqualTo("CheckedChanged Triggered"));
		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed"));
	}

	[Test, Order(13)]
	public void CheckBox_DirectTap_TogglesIsChecked()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

	[Test, Order(14)]
	public void CheckBox_VerifyIsCheckedAfterReset()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

	[Test, Order(15)]
	public void CheckBox_VerifyWithShadow()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(HasShadowCheckBox);
		App.Tap(HasShadowCheckBox);
		App.WaitForElement(CheckBoxControl);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	public void CheckBox_VerifyCheckedWhileIsEnableSetFalseUsingTap()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
	}

	[Test, Order(17)]
	public void CheckBox_VerifyCheckedWhileIsEnableSetFalseUsingButton()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));
	}

	[Test, Order(18)]
	public void CheckBox_VerifyUnCheckedWhileIsEnableSetFalseUsingTap()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

	[Test, Order(19)]
	public void CheckBox_VerifyUnCheckedWhileIsEnableSetFalseUsingButton()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);
		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

}