using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.VisualStateManager)]
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
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyVSM_Entry_Focus()
	{
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
#if ANDROID
		VerifyScreenshot(cropBottom: 1150);
		if(App.IsKeyboardShown())
			App.DismissKeyboard();
#elif IOS
		VerifyScreenshot(cropBottom: 1200);
		if(App.IsKeyboardShown())
			App.DismissKeyboard();
#else
		VerifyScreenshot();
#endif
	}

	[Test, Order(3)]
	public void VerifyVSM_Entry_Unfocus()
	{
		App.WaitForElement("NormalEntryButton");
		App.Tap("NormalEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal/Unfocused"));
		VerifyScreenshot();
	}

	[Test, Order(4)]
	public void VerifyVSM_Entry_Disable()
	{
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
	}

	[Test, Order(5)]
	public void VerifyVSM_Entry_Reset()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		VerifyScreenshot();
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
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		App.Tap("FocusEntryButton");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
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
		VerifyScreenshot();
	}

	[Test, Order(8)]
	public void VerifyVSM_Entry_DisableWhileReset()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot();
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
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		App.Tap("FocusEntryButton");
		App.WaitForElement("EntryState");
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
		VerifyScreenshot();
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
		VerifyScreenshot();
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
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Valid"));
		VerifyScreenshot();
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
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Invalid"));
		VerifyScreenshot();
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
		if (App.IsKeyboardShown())
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
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		var stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Valid"));
		App.WaitForElement("ValidateEntryButton");
		App.Tap("ValidateEntryButton");
		App.ClearText("ValidationEntry");
		App.EnterText("ValidationEntry", "6789-456-1234");
#if ANDROID || IOS
		if (App.IsKeyboardShown())
			App.DismissKeyboard();
#endif
		App.WaitForElement("ValidationEntryLabel");
		stateText = App.FindElement("ValidationEntryLabel").GetText();
		Assert.That(stateText, Is.EqualTo("State: Invalid"));
	}
#if TEST_FAILS_ON_ANDROID // When PressEnter() is triggered to verify the Completed state, the Entry automatically becomes visually unfocused. Therefore, the test currently fails on Android in automation but passes during manual testing.
	[Test, Order(19)]
	public void VerifyVSM_Entry_Completed()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.EnterText("VSMEntry", "Hello");
		App.PressEnter();
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Completed"));
		VerifyScreenshot();
	}

	[Test, Order(20)]
	public void VerifyVSM_Entry_CompletedAndReset()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.EnterText("VSMEntry", "Hello");
		App.PressEnter();
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Completed"));
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Normal"));
	}

	[Test, Order(21)]
	public void VerifyVSM_Entry_CompletedAndRefocused()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.EnterText("VSMEntry", "Hello");
		App.PressEnter();
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Completed"));
		App.WaitForElement("FocusEntryButton");
		App.Tap("FocusEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
	}

	[Test, Order(22)]
	public void VerifyVSM_Entry_DisableWhileCompleted()
	{
		App.WaitForElement("ResetEntryButton");
		App.Tap("ResetEntryButton");
		App.WaitForElement("VSMEntry");
		App.Tap("VSMEntry");
		App.EnterText("VSMEntry", "Hello");
		App.PressEnter();
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Completed"));
		App.WaitForElement("DisableEntryButton");
		App.Tap("DisableEntryButton");
		stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
	}
#endif
}

