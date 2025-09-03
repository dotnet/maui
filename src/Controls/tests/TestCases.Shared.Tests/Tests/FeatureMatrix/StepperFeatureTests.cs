using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Stepper)]
public class StepperFeatureTests : _GalleryUITest
{
	public const string StepperFeatureMatrix = "Stepper Feature Matrix";

	public override string GalleryPageName => StepperFeatureMatrix;

	public StepperFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void Stepper_ValidateDefaultValues_VerifyLabels()
	{
		App.WaitForElement("Options");
		Assert.That(App.FindElement("MinimumLabel").GetText(), Is.EqualTo("0.00"));
		Assert.That(App.FindElement("MaximumLabel").GetText(), Is.EqualTo("10.00"));
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("0.00"));
	}

	[Test]
	public void Stepper_SetMinimumValue_VerifyMinimumLabel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinimumEntry");
		App.EnterText("MinimumEntry", "2");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		Assert.That(App.FindElement("MinimumLabel").GetText(), Is.EqualTo("2.00"));
	}

	[Test]
	public void Stepper_SetMaximumValue_VerifyMaximumLabel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaximumEntry");
		App.EnterText("MaximumEntry", "20");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		Assert.That(App.FindElement("MaximumLabel").GetText(), Is.EqualTo("20.00"));
	}

	[Test]
	public void Stepper_SetValueWithinRange_VerifyValueLabel()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ValueEntry");
		App.EnterText("ValueEntry", "5");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("5.00"));
	}

	[Test]
	public void Stepper_SetIncrementValue_VerifyIncrement()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IncrementEntry");
		App.EnterText("IncrementEntry", "2");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.IncreaseStepper("StepperControl");
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("2.00"));
	}

	[Test]
	public void Stepper_SetValueExceedsMaximum()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MaximumEntry");
		App.ClearText("MaximumEntry");
		App.EnterText("MaximumEntry", "100");
		App.PressEnter();
		App.ClearText("ValueEntry");
		App.EnterText("ValueEntry", "200");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("100.00"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS
// Related Issue Link : https://github.com/dotnet/maui/issues/12243
		[Test]
		public void Stepper_SetValueBelowMinimum()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.ClearText("MinimumEntry");
			App.EnterText("MinimumEntry", "10");
			App.PressEnter();
			App.ClearText("ValueEntry");
			App.EnterText("ValueEntry", "5");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");
			Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("10.00"));
		}

		[Test]
		public void Stepper_MinimumExceedsMaximum_SetsMinimumToMaximum()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("MinimumEntry");
			App.ClearText("MinimumEntry");
			App.EnterText("MinimumEntry", "50");
			App.PressEnter();
			App.ClearText("MaximumEntry");
			App.EnterText("MaximumEntry", "25");
			App.PressEnter();
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Options");
			Assert.That(App.FindElement("MinimumLabel").GetText(), Is.EqualTo("50.00"));
			Assert.That(App.FindElement("MaximumLabel").GetText(), Is.EqualTo("50.00"));
		}
#endif

	[Test]
	public void Stepper_SetEnabledStateToFalse_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseRadio");
		App.Tap("IsEnabledFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.IncreaseStepper("StepperControl");
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("0.00"));
	}

	[Test]
	public void Stepper_SetVisibilityToFalse_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalseRadio");
		App.Tap("IsVisibleFalseRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.WaitForNoElement("StepperControl");
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST     //Related Issue Link : https://github.com/dotnet/maui/issues/29704
	[Test]
	public void Stepper_ChangeFlowDirection_RTL_VerifyVisualState()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRTLRadio");
		App.Tap("FlowDirectionRTLRadio");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		VerifyScreenshot();
	}
#endif
	[Test]
	public void Stepper_AtMinimumValue_DecrementButtonDisabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("MinimumEntry");
		App.ClearText("MinimumEntry");
		App.EnterText("MinimumEntry", "10");
		App.PressEnter();

		App.WaitForElement("ValueEntry");
		App.ClearText("ValueEntry");
		App.EnterText("ValueEntry", "10");
		App.PressEnter();

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("Options");

		var currentValue = App.FindElement("ValueLabel").GetText();
		Assert.That(currentValue, Is.EqualTo("10.00"));

		App.DecreaseStepper("StepperControl");

		var newValue = App.FindElement("ValueLabel").GetText();
		Assert.That(newValue, Is.EqualTo("10.00"));
	}

	[Test]
	public void Stepper_AtMaximumValue_IncrementButtonDisabled()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("MaximumEntry");
		App.EnterText("MaximumEntry", "10");
		App.PressEnter();

		App.WaitForElement("ValueEntry");
		App.EnterText("ValueEntry", "10");
		App.PressEnter();

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("Options");

		var currentValue = App.FindElement("ValueLabel").GetText();
		Assert.That(currentValue, Is.EqualTo("10.00"));

		App.IncreaseStepper("StepperControl");

		var newValue = App.FindElement("ValueLabel").GetText();
		Assert.That(newValue, Is.EqualTo("10.00"));
	}

	[Test]
	public void Stepper_SetIncrementAndVerifyValueChange()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IncrementEntry");
		App.EnterText("IncrementEntry", "5");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.IncreaseStepper("StepperControl");
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("5.00"));
		App.IncreaseStepper("StepperControl");
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("10.00"));
	}


	[Test]
	public void Stepper_ResetToInitialState_VerifyDefaultValues()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("MinimumEntry");
		App.EnterText("MinimumEntry", "10");
		App.PressEnter();
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Options");
		Assert.That(App.FindElement("MinimumLabel").GetText(), Is.EqualTo("0.00"));
		Assert.That(App.FindElement("MaximumLabel").GetText(), Is.EqualTo("10.00"));
		Assert.That(App.FindElement("ValueLabel").GetText(), Is.EqualTo("0.00"));
	}

#if TEST_FAILS_ON_WINDOWS // Related Issue Link : https://github.com/dotnet/maui/issues/29740
	[Test]
	public void Stepper_IncrementDoesNotExceedMaximum()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("MaximumEntry");
		App.ClearText("MaximumEntry");
		App.EnterText("MaximumEntry", "10");
		App.PressEnter();

		App.WaitForElement("IncrementEntry");
		App.ClearText("IncrementEntry");
		App.EnterText("IncrementEntry", "3");
		App.PressEnter();

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("Options");

		App.IncreaseStepper("StepperControl");
		App.IncreaseStepper("StepperControl");
		App.IncreaseStepper("StepperControl");
		App.IncreaseStepper("StepperControl");

		var currentValue = App.FindElement("ValueLabel").GetText();
		Assert.That(currentValue, Is.EqualTo("10.00"));
	}
#endif

	[Test]
	public void Stepper_DecrementDoesNotGoBelowMinimum()
	{
		App.WaitForElement("Options");
		App.Tap("Options");

		App.WaitForElement("MinimumEntry");
		App.ClearText("MinimumEntry");
		App.EnterText("MinimumEntry", "0");
		App.PressEnter();

		App.WaitForElement("IncrementEntry");
		App.ClearText("IncrementEntry");
		App.EnterText("IncrementEntry", "2");
		App.PressEnter();

		App.WaitForElement("ValueEntry");
		App.ClearText("ValueEntry");
		App.EnterText("ValueEntry", "2");
		App.PressEnter();

		App.WaitForElement("Apply");
		App.Tap("Apply");

		App.WaitForElement("Options");

		App.DecreaseStepper("StepperControl");
		App.DecreaseStepper("StepperControl");

		var currentValue = App.FindElement("ValueLabel").GetText();
		Assert.That(currentValue, Is.EqualTo("0.00"));
	}
}