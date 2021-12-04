#nullable disable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, MauiStepper>
	{
		protected override MauiStepper CreateNativeView() => new MauiStepper();

		protected override void ConnectHandler(MauiStepper nativeView)
		{
			nativeView.ValueChanged += OnValueChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiStepper nativeView)
		{
			nativeView.ValueChanged -= OnValueChanged;

			base.DisconnectHandler(nativeView);
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
			handler.NativeView?.UpdateInterval(stepper); 
		}

		public static void MapValue(StepperHandler handler, IStepper stepper) 
		{ 
			handler.NativeView?.UpdateValue(stepper); 
		}

		// This is a Windows-specific mapping
		public static void MapBackground(StepperHandler handler, IStepper view)
		{
			handler.NativeView?.UpdateBackground(view);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Value = NativeView.Value;
		}
	}
}