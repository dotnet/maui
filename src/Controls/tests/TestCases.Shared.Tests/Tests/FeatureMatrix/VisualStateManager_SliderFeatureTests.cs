using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_SliderFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerSliderFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerSliderFeatureTests;

	public VisualStateManager_SliderFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void VerifyVSM_Slider_Unfocus_UpdatesStateLabel()
	{
		App.WaitForElement("VSMSliderButton");
		App.Tap("VSMSliderButton");
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderUnfocus");
		App.Tap("SliderUnfocus");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused | Value: 50"));
		VerifyScreenshot("Slider_Unfocused_State");
	}

	[Test, Order(2)]
	public void VerifyVSM_Slider_Reset_ShowsUnfocused()
	{
		App.WaitForElement("SliderReset");
		App.Tap("SliderReset");
		App.WaitForElement("SliderState");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused | Value: 50"));
		VerifyScreenshot("Slider_Reset_Unfocused_State");
	}

	[Test, Order(3)]
	public void VerifyVSM_Slider_Disable_ShowsDisabledWithValue()
	{
		App.WaitForElement("SliderDisable");
		App.Tap("SliderDisable");
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Disabled"));
		Assert.That(stateText, Does.Contain("Value:"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	public void VerifyVSM_Slider_Unfocus_UpdatesToUnfocusedState()
	{
		App.WaitForElement("SliderFocus");
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));

		App.Tap("SliderState");
		System.Threading.Thread.Sleep(500);

		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Unfocused"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(5)]
	public void VerifyVSM_Slider_DisableWhileFocused_ShowsDisabled()
	{
		App.WaitForElement("SliderFocus");

		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));

		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);

		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	public void VerifyVSM_Slider_DisableWhileUnfocused_ShowsDisabled()
	{
		App.WaitForElement("SliderReset");

		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));

		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);

		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Disabled"));

		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	public void VerifyVSM_Slider_EnableAfterDisable_RestoresUnfocusedState()
	{
		App.WaitForElement("SliderDisable");
		
		// Disable
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Disabled"));
		
		// Enable - focus is lost when disabled
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);
		
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Unfocused"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	public void VerifyVSM_Slider_ResetFromDisabled_RestoresUnfocused()
	{
		App.WaitForElement("SliderFocus");
		
		// Focus and disable
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(300);
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Disabled"));
		
		// Reset
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(300);
		
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Unfocused"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	public void VerifyVSM_Slider_MultipleFocusUnfocusCycles()
	{
		App.WaitForElement("SliderFocus");
		
		// Multiple focus/unfocus cycles
		for (int i = 0; i < 3; i++)
		{
			App.Tap("SliderFocus");
			System.Threading.Thread.Sleep(300);
			Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));
			
			App.Tap("SliderState");
			System.Threading.Thread.Sleep(300);
			Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		}
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	public void VerifyVSM_Slider_SequentialStateTransitions_Unfocused_Focused_Disabled()
	{
		App.WaitForElement("SliderReset");
		
		// Unfocused
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		// Focused
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));
		
		// Disabled
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Disabled"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	public void VerifyVSM_Slider_SequentialStateTransitions_Disabled_Unfocused_Focused()
	{
		App.WaitForElement("SliderDisable");
		
		// Disabled
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Disabled"));
		
		// Unfocused via Reset
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		// Focused
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	public void VerifyVSM_Slider_AllStateTransitions_VerifyVisualChanges()
	{
		App.WaitForElement("SliderReset");
		
		// Complete cycle: Unfocused -> Focused -> Unfocused -> Disabled -> Unfocused
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));
		
		App.Tap("SliderState");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Disabled"));
		
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	public void VerifyVSM_Slider_RapidFocusChanges_HandlesCorrectly()
	{
		App.WaitForElement("SliderFocus");
		
		// Rapid focus/unfocus
		for (int i = 0; i < 5; i++)
		{
			App.Tap("SliderFocus");
			System.Threading.Thread.Sleep(100);
			App.Tap("SliderState");
			System.Threading.Thread.Sleep(100);
		}
		
		System.Threading.Thread.Sleep(500);
		var stateText = App.FindElement("SliderState").GetText();
		Assert.That(stateText, Does.Contain("State: Unfocused"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	public void VerifyVSM_Slider_EdgeCase_ToggleDisableMultipleTimes()
	{
		App.WaitForElement("SliderDisable");
		
		// Toggle disable multiple times
		for (int i = 0; i < 4; i++)
		{
			App.Tap("SliderDisable");
			System.Threading.Thread.Sleep(200);
			var expectedState = (i % 2 == 0) ? "State: Disabled" : "State: Unfocused";
			Assert.That(App.FindElement("SliderState").GetText(), Does.Contain(expectedState));
		}
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	public void VerifyVSM_Slider_EdgeCase_ResetMultipleTimes()
	{
		App.WaitForElement("SliderReset");
		
		// Reset multiple times
		for (int i = 0; i < 3; i++)
		{
			App.Tap("SliderReset");
			System.Threading.Thread.Sleep(200);
			Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		}
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	public void VerifyVSM_Slider_ComplexScenario_MultipleOperations()
	{
		App.WaitForElement("SliderReset");
		
		// Complex: Reset -> Focus -> Unfocus -> Focus -> Disable -> Enable -> Reset
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));
		
		App.Tap("SliderState");
		System.Threading.Thread.Sleep(300);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		App.Tap("SliderFocus");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Focused"));
		
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Disabled"));
		
		App.Tap("SliderDisable");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		App.Tap("SliderReset");
		System.Threading.Thread.Sleep(200);
		Assert.That(App.FindElement("SliderState").GetText(), Does.Contain("State: Unfocused"));
		
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}