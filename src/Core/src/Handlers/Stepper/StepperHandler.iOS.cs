using System;
using System.Drawing;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, UIStepper>
	{
		protected override UIStepper CreateNativeView()
		{
			return new UIStepper(RectangleF.Empty);
		}

		protected override void ConnectHandler(UIStepper nativeView)
		{
			nativeView.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(UIStepper nativeView)
		{
			nativeView.ValueChanged -= OnValueChanged;
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
			if (NativeView == null || VirtualView == null)
				return;

			VirtualView.Value = NativeView.Value;
		}
	}
}