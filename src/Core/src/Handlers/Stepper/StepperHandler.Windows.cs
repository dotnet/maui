#nullable disable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, MauiStepper>
	{
		protected override MauiStepper CreatePlatformView() => new MauiStepper();

		protected override void ConnectHandler(MauiStepper platformView)
		{
			platformView.ValueChanged += OnValueChanged;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiStepper platformView)
		{
			platformView.ValueChanged -= OnValueChanged;

			base.DisconnectHandler(platformView);
		}

		public static void MapMinimum(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateMinimum(stepper);
		}

		public static void MapMaximum(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateMaximum(stepper);
		}

		public static void MapIncrement(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateInterval(stepper);
		}

		public static void MapValue(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateValue(stepper);
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IStepperHandler handler, IStepper view)
		{
			handler.PlatformView?.UpdateBackground(view);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			if (VirtualView.Value != PlatformView.Value)
			{
				VirtualView.Value = PlatformView.Value;
			}
		}
	}
}