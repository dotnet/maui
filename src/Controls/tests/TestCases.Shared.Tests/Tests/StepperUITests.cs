using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Trait("Category", UITestCategories.Stepper)]
	public class StepperUITests : CoreGalleryBasePageTest
	{
		public const string StepperGallery = "Stepper Gallery";

		public StepperUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(StepperGallery);
		}

		[Fact]
		[Trait("Category", UITestCategories.Stepper)]
		public void IncreaseStepper()
		{
			const string titleAutomationId = "DefaultLabel";
			const string stepperAutomationId = "DefaultStepper";
			const string valueAutomationId = "DefaultLabelValue";

			App.WaitForElement(titleAutomationId);

			// 1. Check the current value.
			var step1Value = App.FindElement(valueAutomationId).GetText();
			Assert.Equal("0", step1Value);

			// 2. Increase the value.
			App.IncreaseStepper(stepperAutomationId);

			// 3. Verify that the value has increased.
			var step3Value = App.FindElement(valueAutomationId).GetText();
			Assert.Equal("1", step3Value);
		}

		[Fact]
		[Trait("Category", UITestCategories.Stepper)]
		public void DecreaseStepper()
		{
			const string titleAutomationId = "DefaultLabel";
			const string stepperAutomationId = "DefaultStepper";
			const string valueAutomationId = "DefaultLabelValue";

			App.WaitForElement(titleAutomationId);

			// 1. Check the current value.
			var step1Value = App.FindElement(valueAutomationId).GetText();
			Assert.Equal("0", step1Value);

			// 2. Increase the value.
			App.IncreaseStepper(stepperAutomationId);

			// 3. Verify that the value has increased.
			var step3Value = App.FindElement(valueAutomationId).GetText();
			Assert.Equal("1", step3Value);

			// 4. Decrease the value.
			App.DecreaseStepper(stepperAutomationId);

			// 5. Verify that the value has decreased.
			var step5Value = App.FindElement(valueAutomationId).GetText();
			Assert.Equal("0", step5Value);
		}
	}
}