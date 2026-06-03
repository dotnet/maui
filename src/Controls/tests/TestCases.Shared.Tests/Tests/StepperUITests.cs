using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.Stepper)]
	public class StepperUITests : CoreGalleryBasePageTest
	{
		public const string StepperGallery = "Stepper Gallery";

		public override string GalleryPageName => StepperGallery;
		
		public StepperUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(StepperGallery);
		}

		[Test]
		[Description("Increase the Stepper value")]
		public void IncreaseStepper()
		{
			const string titleAutomationId = "DefaultLabel";
			const string stepperAutomationId = "DefaultStepper";
			const string valueAutomationId = "DefaultLabelValue";

			App.WaitForElement(titleAutomationId);

			// 1. Check the current value.
			var step1Value = App.FindElement(valueAutomationId).GetText();
			ClassicAssert.AreEqual("0", step1Value);

			// 2. Increase the value and verify - retry tap if it didn't register.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
			App.RetryAssert(() =>
			{
				var currentValue = App.FindElement(valueAutomationId).GetText();
				if (currentValue != "1")
				{
#if MACCATALYST
					App.DecreaseStepper(stepperAutomationId);
#else
					App.IncreaseStepper(stepperAutomationId);
#endif
					currentValue = App.FindElement(valueAutomationId).GetText();
				}
				ClassicAssert.AreEqual("1", currentValue);
			});
		}

		[Test]
		[Description("Decrease the Stepper value")]
		public void DecreaseStepper()
		{
			const string titleAutomationId = "DefaultLabel";
			const string stepperAutomationId = "DefaultStepper";
			const string valueAutomationId = "DefaultLabelValue";

			App.WaitForElement(titleAutomationId);

			// 1. Check the current value.
			var step1Value = App.FindElement(valueAutomationId).GetText();
			ClassicAssert.AreEqual("0", step1Value);

			// 2. Increase the value and verify - retry tap if it didn't register.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
			App.RetryAssert(() =>
			{
				var currentValue = App.FindElement(valueAutomationId).GetText();
				if (currentValue != "1")
				{
#if MACCATALYST
					App.DecreaseStepper(stepperAutomationId);
#else
					App.IncreaseStepper(stepperAutomationId);
#endif
					currentValue = App.FindElement(valueAutomationId).GetText();
				}
				ClassicAssert.AreEqual("1", currentValue);
			});

			// 3. Decrease the value and verify - retry tap if it didn't register.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
			App.RetryAssert(() =>
			{
				var currentValue = App.FindElement(valueAutomationId).GetText();
				if (currentValue != "0")
				{
#if MACCATALYST
					App.IncreaseStepper(stepperAutomationId);
#else
					App.DecreaseStepper(stepperAutomationId);
#endif
					currentValue = App.FindElement(valueAutomationId).GetText();
				}
				ClassicAssert.AreEqual("0", currentValue);
			});
		}
	}
}