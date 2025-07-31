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
		[Category(UITestCategories.Stepper)]
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

			// 2. Increase the value.
			App.IncreaseStepper(stepperAutomationId);

			// 3. Verify that the value has increased.
			var step3Value = App.FindElement(valueAutomationId).GetText();
			ClassicAssert.AreEqual("1", step3Value);
		}

		[Test]
		[Category(UITestCategories.Stepper)]
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

			// 2. Increase the value.
			App.IncreaseStepper(stepperAutomationId);

			// 3. Verify that the value has increased.
			var step3Value = App.FindElement(valueAutomationId).GetText();
			ClassicAssert.AreEqual("1", step3Value);

			// 4. Decrease the value.
			App.DecreaseStepper(stepperAutomationId);

			// 5. Verify that the value has decreased.
			var step5Value = App.FindElement(valueAutomationId).GetText();
			ClassicAssert.AreEqual("0", step5Value);
		}
	}
}