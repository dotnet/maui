#nullable disable
namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, MauiStepper>
	{
		protected override MauiStepper CreateNativeView() => new MauiStepper();

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
	}
}