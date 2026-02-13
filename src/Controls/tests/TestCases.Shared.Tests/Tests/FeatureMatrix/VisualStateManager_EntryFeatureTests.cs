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
	public void VerifyVSM_Entry_Focus_UpdatesStateLabel()
	{
		App.WaitForElement("VSMEntryButton");
		App.Tap("VSMEntryButton");
		App.WaitForElement("ResetEntry");
		App.Tap("ResetEntry");
		App.WaitForElement("EntryFocus");
		App.Tap("EntryFocus");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		VerifyScreenshot("Entry_Focused_State");
	}

	[Test, Order(2)]
	public void VerifyVSM_Entry_Disable_UpdatesStateLabel()
	{
		App.WaitForElement("ResetEntry");
		App.Tap("ResetEntry");
		App.WaitForElement("EntryDisable");
		App.Tap("EntryDisable");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot("Entry_Disabled_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_Entry_Enable_ShowsUnfocused()
	{
		App.WaitForElement("ResetEntry");
		App.Tap("ResetEntry");
		App.WaitForElement("EntryDisable");
		App.Tap("EntryDisable");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused"));
		VerifyScreenshot("Entry_Unfocused_State");
	}

	[Test, Order(4)]
	public void VerifyVSM_Entry_TapToFocus_UpdatesState()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("DemoEntry");

		App.Tap("DemoEntry");
		System.Threading.Thread.Sleep(500);

		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	public void VerifyVSM_Entry_Unfocus_UpdatesState()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("DemoEntry");

		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		App.Tap("EntryState");
		System.Threading.Thread.Sleep(500);

		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	public void VerifyVSM_Entry_DisableWhileFocused_ShowsDisabled()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryFocus");

		// Focus first
		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		// Disable
		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);

		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	public void VerifyVSM_Entry_DisableWhileUnfocused_ShowsDisabled()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryDisable");

		App.Tap("EntryState");
		System.Threading.Thread.Sleep(300);

		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);

		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	public void VerifyVSM_Entry_EnableWhileFocusedState_RestoresFocused()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryFocus");

		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(300);
		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Disabled"));

		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);

		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	public void VerifyVSM_Entry_MultipleFocusUnfocusCycles()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryFocus");

		// Multiple focus/unfocus cycles
		for (int i = 0; i < 3; i++)
		{
			App.Tap("EntryFocus");
			System.Threading.Thread.Sleep(300);
			Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

			App.Tap("EntryState");
			System.Threading.Thread.Sleep(300);
			Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));
		}

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	public void VerifyVSM_Entry_SequentialStateTransitions_Unfocused_Focused_Disabled()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryState");

		// Start unfocused
		App.Tap("EntryState");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		// Transition to Focused
		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		// Transition to Disabled
		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	public void VerifyVSM_Entry_SequentialStateTransitions_Disabled_Unfocused_Focused()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryDisable");

		// Start disabled
		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Disabled"));

		// Enable to Unfocused
		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		// Transition to Focused
		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	public void VerifyVSM_Entry_AllStateTransitions_VerifyVisualChanges()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryFocus");

		// Complete cycle: Unfocused -> Focused -> Unfocused -> Disabled -> Unfocused
		App.Tap("EntryState");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		App.Tap("EntryState");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Disabled"));

		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	public void VerifyVSM_Entry_RapidFocusChanges_HandlesCorrectly()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryFocus");

		// Rapid focus/unfocus
		for (int i = 0; i < 5; i++)
		{
			App.Tap("EntryFocus");
			System.Threading.Thread.Sleep(100);
			App.Tap("EntryState");
			System.Threading.Thread.Sleep(100);
		}

		System.Threading.Thread.Sleep(500);
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	public void VerifyVSM_Entry_EdgeCase_ToggleDisableMultipleTimes()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryDisable");

		// Toggle disable multiple times
		for (int i = 0; i < 4; i++)
		{
			App.Tap("EntryDisable");
			System.Threading.Thread.Sleep(200);
			var expectedState = (i % 2 == 0) ? "State: Disabled" : "State: Unfocused";
			Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo(expectedState));
		}

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	public void VerifyVSM_Entry_ComplexScenario_MultipleOperations()
	{
		try
		{ App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); }
		catch { }
		App.WaitForElement("EntryFocus");

		// Complex: Unfocus -> Focus -> Unfocus -> Focus -> Disable -> Enable -> Focus
		App.Tap("EntryState");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		App.Tap("EntryState");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Disabled"));

		App.Tap("EntryDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Unfocused"));

		App.Tap("EntryFocus");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("EntryState").GetText(), Is.EqualTo("State: Focused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}