using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class CheckBoxFeatureTests : UITest
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

	public const string CheckBoxFeatureMatrix = "CheckBox Feature Matrix";

	public CheckBoxFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(CheckBoxFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_ValidateDefaultValues_VerifyLabels()
	{
		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
		App.WaitForElement(CheckBoxControl);
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
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

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_VerifyCheckedChangedEvent()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);

		App.Tap(CheckBoxControl);

		App.WaitForNoElement("CheckedChanged Triggered");

		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);

		App.Tap(CheckBoxControl);

		Assert.That(App.FindElement(CheckedChangedStatusLabel).GetText(), Is.EqualTo("CheckedChanged Triggered"));
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
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

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_ChangeColor_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement("GreenColorButton");
		App.Tap("GreenColorButton");

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_SetIsCheckedAndColor_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement(IsCheckedSwitch);
		App.Tap(IsCheckedSwitch);

		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);

		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("False"));

		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_SetAllProperties_VerifyVisualState()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement(BlueColorButton);
		App.Tap(BlueColorButton);

		App.WaitForElement(IsEnabledSwitch);
		App.Tap(IsEnabledSwitch);

		App.Tap(CheckBoxControl);

		Assert.That(App.FindElement(IsCheckedLabel).GetText(), Is.EqualTo("True"));
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_VerifyCommandExecution()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);

		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed"));
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_VerifyCommandWithParameter()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement("CommandParameterEntry");
		App.EnterText("CommandParameterEntry", "TestParameter");

		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);

		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed: TestParameter"));
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
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

	[Test, Order(11)]
	[Category(UITestCategories.CheckBox)]
	public void CheckBox_VerifyBothEventAndCommandExecuted()
	{
		App.WaitForElement(ResetButton);
		App.Tap(ResetButton);

		App.WaitForElement(CheckBoxControl);
		App.Tap(CheckBoxControl);

		Assert.That(App.FindElement(CheckedChangedStatusLabel).GetText(), Is.EqualTo("CheckedChanged Triggered"));
		Assert.That(App.FindElement(CommandStatusLabel).GetText(), Is.EqualTo("Command Executed"));
	}
}
