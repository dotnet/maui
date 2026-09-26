using System;
using System.Threading.Tasks;
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

		[Theory]
		[InlineData(UIInterfaceOrientation.LandscapeLeft, 390, 844, true)]
		[InlineData(UIInterfaceOrientation.LandscapeRight, 390, 844, true)]
		[InlineData(UIInterfaceOrientation.Portrait, 844, 390, false)]
		[InlineData(UIInterfaceOrientation.PortraitUpsideDown, 844, 390, false)]
		[InlineData(UIInterfaceOrientation.Unknown, 844, 390, true)]
		[InlineData(UIInterfaceOrientation.Unknown, 390, 844, false)]
		public void LandscapeDetectionPrefersSceneOrientation(
			UIInterfaceOrientation orientation,
			double screenWidth,
			double screenHeight,
			bool expected)
		{
			var screenBounds = new CoreGraphics.CGRect(0, 0, screenWidth, screenHeight);

			Assert.Equal(expected, StepperHandler.IsLandscape(orientation, screenBounds));
		}
	}
}