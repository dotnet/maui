using System;
using Tizen.UIExtensions.NUI.GraphicsView;


namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, Stepper>
	{
		protected override Stepper CreatePlatformView() => new Stepper();

		protected override void ConnectHandler(Stepper nativeView)
		{
			platformView!.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(Stepper platformView)
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