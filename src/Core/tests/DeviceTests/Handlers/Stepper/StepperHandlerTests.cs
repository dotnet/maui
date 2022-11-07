using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Stepper)]
	public partial class StepperHandlerTests : CoreHandlerTestBase<StepperHandler, StepperStub>
	{
		[Fact(DisplayName = "Is Value Initializes Correctly")]
		public async Task ValueInitializesCorrectly()
		{
			var stepper = new StepperStub()
			{
				Maximum = 100,
				Minimum = 0,
				Value = 50
			};

			await ValidatePropertyInitValue(stepper, () => stepper.Value, GetPlatformValue, stepper.Value);
		}

		[Fact(DisplayName = "Is Maximum Initializes Correctly")]
		public async Task MaximumInitializesCorrectly()
		{
			var stepper = new StepperStub()
			{
				Minimum = 0,
				Maximum = 50
			};

			await ValidatePropertyInitValue(stepper, () => stepper.Maximum, GetNativeMaximum, stepper.Maximum);
		}

		[Fact(DisplayName = "Is Minimum Initializes Correctly")]
		public async Task MinimumInitializesCorrectly()
		{
			var stepper = new StepperStub()
			{
				Minimum = 10,
				Maximum = 50
			};

			await ValidatePropertyInitValue(stepper, () => stepper.Minimum, GetNativeMinimum, stepper.Minimum);
		}

		[Fact(DisplayName = "Background Initializes Correctly")]
		public async Task BackgroundInitializesCorrectly()
		{
			var stepper = new StepperStub()
			{
				Background = new SolidPaintStub(Colors.Red)
			};

			await ValidateHasColor(stepper, Colors.Red);
		}
	}
}