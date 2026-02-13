using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_LabelFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerLabelFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerLabelFeatureTests;

	public VisualStateManager_LabelFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Label_Selected_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");
		App.WaitForElement("LabelState");
		App.Tap("SelectableLabelContainer");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Selected"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void VerifyVSM_Label_Disable_BlocksSelectionAndUpdatesStateLabel()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelDisable");
		App.Tap("LabelDisable");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));
		// Tap container should not change state while disabled
		App.Tap("SelectableLabelContainer");
		labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	public void VerifyVSM_Label_Reset_ReturnsToNormal()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");
		App.Tap("LabelReset");
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	public void VerifyVSM_Label_Deselect_ReturnsToNormal()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");

		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));

		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	public void VerifyVSM_Label_DisableWhileSelected_ShowsDisabled()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");

		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));

		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	public void VerifyVSM_Label_DisableWhileNormal_ShowsDisabled()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");

		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));

		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	public void VerifyVSM_Label_EnableWhileSelected_RestoresSelected()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");

		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));

		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Selected"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	public void VerifyVSM_Label_EnableWhileNormal_RestoresNormal()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");

		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));

		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	public void VerifyVSM_Label_ResetFromDisabled_ClearsAndEnables()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");

		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));

		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Normal"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	public void VerifyVSM_Label_MultipleToggle_StateTransitionsCorrectly()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");
		
		// Multiple select/deselect cycles
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		
		for (int i = 0; i < 3; i++)
		{
			App.Tap("SelectableLabelContainer");
			System.Threading.Thread.Sleep(200);
			Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
			
			App.Tap("SelectableLabelContainer");
			System.Threading.Thread.Sleep(200);
			Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		}
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	public void VerifyVSM_Label_SequentialStateTransitions_Normal_Selected_Disabled()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");
		
		// Normal
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		// Selected
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		// Disabled
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	public void VerifyVSM_Label_SequentialStateTransitions_Disabled_Normal_Selected()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelDisable");
		
		// Disabled
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));
		
		// Normal via Reset
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		// Selected
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	public void VerifyVSM_Label_AllStateTransitions_VerifyVisualChanges()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");
		
		// Complete cycle: Normal -> Selected -> Normal -> Disabled -> Normal
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));
		
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	public void VerifyVSM_Label_SelectionPersistence_AfterDisableEnable()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");
		
		// Select
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		// Disable and enable
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(300);
		
		// Should still be selected
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Selected"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	public void VerifyVSM_Label_RapidSelection_HandlesCorrectly()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("SelectableLabelContainer");
		
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(300);
		
		// Rapid taps
		for (int i = 0; i < 5; i++)
		{
			App.Tap("SelectableLabelContainer");
			System.Threading.Thread.Sleep(100);
		}
		
		System.Threading.Thread.Sleep(500);
		// After 5 taps from normal: should be selected
		var labelText = App.FindElement("LabelState").GetText();
		Assert.That(labelText, Is.EqualTo("State: Selected"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	public void VerifyVSM_Label_EdgeCase_ResetMultipleTimes()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");
		
		// Reset multiple times
		for (int i = 0; i < 3; i++)
		{
			App.Tap("LabelReset");
			System.Threading.Thread.Sleep(200);
			Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		}
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	public void VerifyVSM_Label_EdgeCase_ToggleDisableMultipleTimes()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelDisable");
		
		// Select first
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(300);
		
		// Toggle disable multiple times
		for (int i = 0; i < 3; i++)
		{
			App.Tap("LabelDisable");
			System.Threading.Thread.Sleep(200);
			var expectedState = (i % 2 == 0) ? "State: Disabled" : "State: Selected";
			Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo(expectedState));
		}
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	public void VerifyVSM_Label_ComplexScenario_MultipleOperations()
	{
		try { App.WaitForElement("VSMLabelButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMLabelButton"); } catch { }
		App.WaitForElement("LabelReset");
		
		// Complex: Reset -> Select -> Deselect -> Select -> Disable -> Enable -> Reset
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		App.Tap("SelectableLabelContainer");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Disabled"));
		
		App.Tap("LabelDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Selected"));
		
		App.Tap("LabelReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("LabelState").GetText(), Is.EqualTo("State: Normal"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}