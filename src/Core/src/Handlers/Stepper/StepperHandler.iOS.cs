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

			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView)
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		public static void MapMaximum(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateMaximum(stepper);

			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView)
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		public static void MapIncrement(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateIncrement(stepper);
			
			// When increment changes, Adjust stepValue for boundary handling
			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView)
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		public static void MapValue(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateValue(stepper);

			// iOS 26+ fix: Adjust stepValue for boundary handling
			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView)
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		static void AdjustStepValueForBoundaries(IStepper virtualView, UIStepper platformView)
		{
			var originalIncrement = virtualView.Interval;

			// Only proceed if we have a valid positive increment
			if (originalIncrement <= 0)
			{
				return;
			}

			var currentValue = virtualView.Value;
			var minimum = virtualView.Minimum;
			var maximum = virtualView.Maximum;

			// Validate range configuration - if invalid, fallback to original increment
			if (maximum <= minimum)
			{
				platformView.StepValue = originalIncrement;
				return;
			}

			// Clamp current value to valid range if needed
			currentValue = Math.Max(minimum, Math.Min(maximum, currentValue));

			var spaceToMax = maximum - currentValue;
			var spaceToMin = currentValue - minimum;

			// Use small epsilon for floating-point comparison precision
			const double epsilon = 1e-10;

			// Store current stepValue to minimize property access
			var currentStepValue = platformView.StepValue;

			// If we're close to boundaries and increment would exceed them,
			// temporarily reduce stepValue to allow reaching exact boundary
			if (spaceToMax > epsilon && spaceToMax < originalIncrement && Math.Abs(currentStepValue - spaceToMax) > epsilon)
			{
				platformView.StepValue = spaceToMax;
			}
			else if (spaceToMin > epsilon && spaceToMin < originalIncrement && Math.Abs(currentStepValue - spaceToMin) > epsilon)
			{
				platformView.StepValue = spaceToMin;
			}
			else if (Math.Abs(currentStepValue - originalIncrement) > epsilon)
			{
				// Restore original increment when not near boundaries
				platformView.StepValue = originalIncrement;
			}
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
				{
					// iOS 26+ fix: Adjust stepValue for boundary handling
					if (OperatingSystem.IsIOSVersionAtLeast(26))
					{
						AdjustStepValueForBoundaries(virtualView, platformView);
					}

					virtualView.Value = platformView.Value;
				}
			}
		}
	}
}