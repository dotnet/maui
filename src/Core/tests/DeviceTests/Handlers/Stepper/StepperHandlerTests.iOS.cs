using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StepperHandlerTests
	{
		UIStepper GetNativeStepper(StepperHandler stepperHandler) =>
			stepperHandler.PlatformView;

		double GetPlatformValue(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).Value;

		double GetNativeMaximum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).MaximumValue;

		double GetNativeMinimum(StepperHandler stepperHandler) =>
			GetNativeStepper(stepperHandler).MinimumValue;

#if IOS && !MACCATALYST
		[Fact]
		public async Task DesiredSizeUsesOwningWindowBoundsForLandscapeCompensation()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
				return;

			var stepper = new StepperStub();
			var handler = await CreateHandlerAsync(stepper);

			await InvokeOnMainThreadAsync(() =>
			{
#pragma warning disable CA1422 // A scene-less window is intentional to test the detached-scene fallback.
				using var window = new UIWindow(new CoreGraphics.CGRect(0, 0, 844, 390));
#pragma warning restore CA1422
				window.AddSubview(handler.PlatformView);

				var landscapeSize = handler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);

				window.Bounds = new CoreGraphics.CGRect(0, 0, 390, 844);
				var portraitSize = handler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);

				Assert.Equal(36, landscapeSize.Width - portraitSize.Width);
				Assert.Equal(landscapeSize.Height, portraitSize.Height);
			});
		}
#endif
	}
}