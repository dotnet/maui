using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, Spinner>
	{
		protected override Spinner CreatePlatformView() => new Spinner(NativeParent) { IsEditable = false };

		protected override void ConnectHandler(Spinner platformView)
		{
			platformView!.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(Spinner platformView)
		{
			platformView!.ValueChanged -= OnValueChanged;
		}

		public static void MapMinimum(StepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateMinimum(stepper);
		}

		public static void MapMaximum(StepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateMaximum(stepper);
		}

		public static void MapIncrement(StepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateIncrement(stepper);
		}

		public static void MapValue(StepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateValue(stepper);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Value = PlatformView.Value;
		}
	}
}