using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, Microsoft.UI.Xaml.FrameworkElement>
	{
		MauiStepper? GetMauiStepper() => PlatformView as MauiStepper;
		static MauiStepper? GetMauiStepper(IStepperHandler handler) => handler.PlatformView as MauiStepper;

		protected override FrameworkElement CreatePlatformView() => new MauiStepper();

		protected override void ConnectHandler(FrameworkElement platformView)
		{
			if (platformView is MauiStepper mauiStepper)
				mauiStepper.ValueChanged += OnValueChanged;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(FrameworkElement platformView)
		{
			if (platformView is MauiStepper mauiStepper)
				mauiStepper.ValueChanged -= OnValueChanged;

			base.DisconnectHandler(platformView);
		}

		public static void MapMinimum(IStepperHandler handler, IStepper stepper)
		{
			GetMauiStepper(handler)?.UpdateMinimum(stepper);
		}

		public static void MapMaximum(IStepperHandler handler, IStepper stepper)
		{
			GetMauiStepper(handler)?.UpdateMaximum(stepper);
		}

		public static void MapIncrement(IStepperHandler handler, IStepper stepper)
		{
			GetMauiStepper(handler)?.UpdateInterval(stepper);
		}

		public static void MapValue(IStepperHandler handler, IStepper stepper)
		{
			GetMauiStepper(handler)?.UpdateValue(stepper);
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IStepperHandler handler, IStepper view)
		{
			handler.PlatformView?.UpdateBackground(view);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			if (PlatformView is MauiStepper mauiStepper)
				VirtualView.Value = mauiStepper.Value;
		}
	}
}