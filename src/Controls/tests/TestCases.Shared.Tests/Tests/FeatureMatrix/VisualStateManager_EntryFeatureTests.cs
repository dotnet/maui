using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_EntryFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerEntryFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerEntryFeatureTests;

	public VisualStateManager_EntryFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Entry_InitialState()
	{
		App.WaitForElement("VSMEntryButton");
		App.Tap("VSMEntryButton");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Entry_Initial_State");
	}

	[Test, Order(2)]
	public void VerifyVSM_Entry_Focus()
	{
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
#if ANDROID
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		VerifyScreenshot("Entry_Focused_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_Entry_Unfocus()
	{
		App.WaitForElement("NormalEntryButton");
		App.Tap("NormalEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unfocused"));
		VerifyScreenshot("Entry_Unfocused_State");
	}

	[Test, Order(4)]
	public void VerifyVSM_Entry_Disable()
	{
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Entry_Disabled_State");
	}

	[Test, Order(5)]
	public void VerifyVSM_Entry_Reset()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Entry_Reset_State");
	}

	[Test, Order(6)]
	public void VerifyVSM_Entry_DisableWhileFocused()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.EnterText("VSMEntry", "Testing");
		App.WaitForElement("FocusEntryButton");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.Tap("FocusEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Entry_FocusedAndDisabled_State");
	}

	[Test, Order(7)]
	public void VerifyVSM_Entry_DisableWhileUnFocused()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("NormalEntryButton");
		App.Tap("NormalEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unfocused"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Entry_UnfocusedAndDisabled_State");
	}

	[Test, Order(8)]
	public void VerifyVSM_Entry_ResetAfterDisable()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Entry_ResetAndDisabled_State");
	}

	[Test, Order(9)]
	public void VerifyVSM_Entry_DisableAndEnableWhileFocused()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.EnterText("VSMEntry", "Testing");
		App.WaitForElement("FocusEntryButton");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.Tap("FocusEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Entry_Focused_Disable_And_Enable_State");
	}

	[Test, Order(10)]
	public void VerifyVSM_Entry_DisableAndEnableWhileUnFocused()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("NormalEntryButton");
		App.Tap("NormalEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unfocused"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot("Entry_Unfocused_Disable_And_Enable_State");
	}

	[Test, Order(11)]
	public void VerifyVSM_Entry_FocusedAndUnfocused()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		App.WaitForElement("NormalEntryButton");
		App.Tap("NormalEntryButton");
		App.WaitForElement("EntryState");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unfocused"));
	}

	[Test, Order(12)]
	public void VerifyVSM_Entry_ResetWhileDisabled()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("EntryState");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(13)]
	public void VerifyVSM_Entry_FocusedWhileDisabled()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(14)]
	public void VerifyVSM_Entry_UnfocusedWhileDisabled()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		App.WaitForElement("NormalEntryButton");
		App.Tap("NormalEntryButton");
		App.WaitForElement("EntryState");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}

	[Test, Order(15)]
	public void VerifyVSM_Entry_Validate()
	{
		App.WaitForElement("ResetValidationEntryButton");
		App.Tap("ResetValidationEntryButton");
		App.WaitForElement("ValidationEntry");
		App.Tap("ValidationEntry");
		App.EnterText("ValidationEntry", "965-999-9999");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Valid"));
		VerifyScreenshot("Entry_Valid_State");
	}

	[Test, Order(16)]
	public void VerifyVSM_Entry_Invalid()
	{
		App.WaitForElement("ResetValidationEntryButton");
		App.Tap("ResetValidationEntryButton");
		App.WaitForElement("ValidationEntry");
		App.Tap("ValidationEntry");
		App.EnterText("ValidationEntry", "Invalid");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Invalid"));
		VerifyScreenshot("Entry_Invalid_State");
	}

	[Test, Order(17)]
	public void VerifyVSM_Entry_ResetValidation()
	{
		App.WaitForElement("ResetValidationEntryButton");
		App.Tap("ResetValidationEntryButton");
		App.WaitForElement("ValidationEntry");
		App.Tap("ValidationEntry");
		App.EnterText("ValidationEntry", "777-777-7777");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Valid"));
		App.WaitForElement("ResetValidationEntryButton");
		App.Tap("ResetValidationEntryButton");
		App.WaitForElement("ValidationEntryLabel");
		stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Invalid"));
	}

	[Test, Order(18)]
	public void VerifyVSM_Entry_ValidToInvalid()
	{
		App.WaitForElement("ResetValidationEntryButton");
		App.Tap("ResetValidationEntryButton");
		App.WaitForElement("ValidationEntry");
		App.Tap("ValidationEntry");
		App.EnterText("ValidationEntry", "965-999-9999");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Valid"));
		App.WaitForElement("ValidateEntryButton");
		App.Tap("ValidateEntryButton");
		App.EnterText("ValidationEntry", "6789-456-1234");
#if ANDROID || IOS
		if (App.WaitForKeyboardToShow())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Invalid"));
	}
}