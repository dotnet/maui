using System;
using System.Drawing;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, UIStepper>
	{
		readonly StepperProxy _proxy = new();

		protected override UIStepper CreatePlatformView()
		{
			return new UIStepper(RectangleF.Empty);
		}

		protected override void ConnectHandler(UIStepper platformView)
		{
			base.ConnectHandler(platformView);

			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(UIStepper platformView)
		{
			base.DisconnectHandler(platformView);

			_proxy.Disconnect(platformView);
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

		class StepperProxy
		{
			WeakReference<IStepper>? _virtualView;

			IStepper? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IStepper virtualView, UIStepper platformView)
			{
				_virtualView = new(virtualView);
				platformView.ValueChanged += OnValueChanged;
			}

			public void Disconnect(UIStepper platformView)
			{
				platformView.ValueChanged -= OnValueChanged;
			}

			void OnValueChanged(object? sender, EventArgs e)
			{
				if (VirtualView is IStepper virtualView && sender is UIStepper platformView)
					virtualView.Value = platformView.Value;
			}
		}
	}
}