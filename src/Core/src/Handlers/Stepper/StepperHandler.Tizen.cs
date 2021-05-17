using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : EViewHandler<IStepper, Spinner>
	{
		protected override Spinner CreateNativeView() => new Spinner(NativeParent) { IsEditable = false };

		protected override void ConnectHandler(Spinner nativeView)
		{
			nativeView!.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(Spinner nativeView)
		{
			nativeView!.ValueChanged -= OnValueChanged;
		}

		public static void MapMinimum(StepperHandler handler, IStepper stepper)
		{
			handler.NativeView?.UpdateMinimum(stepper);
		}

		public static void MapMaximum(StepperHandler handler, IStepper stepper)
		{
			handler.NativeView?.UpdateMaximum(stepper);
		}

		public static void MapIncrement(StepperHandler handler, IStepper stepper)
		{
			handler.NativeView?.UpdateIncrement(stepper);
		}

		public static void MapValue(StepperHandler handler, IStepper stepper)
		{
			handler.NativeView?.UpdateValue(stepper);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Value = NativeView.Value;
		}
	}
}