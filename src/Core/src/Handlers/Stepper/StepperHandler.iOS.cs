using System;
using System.Drawing;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, UIStepper>
	{
		protected override UIStepper CreatePlatformView()
		{
			return new UIStepper(RectangleF.Empty);
		}

		protected override void ConnectHandler(UIStepper platformView)
		{
			base.ConnectHandler(platformView);

			platformView.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(UIStepper platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.ValueChanged -= OnValueChanged;
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
			handler.PlatformView?.UpdateIncrement(stepper);
		}

		public static void MapValue(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateValue(stepper);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (PlatformView == null || VirtualView == null)
				return;

			VirtualView.Value = PlatformView.Value;
		}
	}
}