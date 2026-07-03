using System;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ViewHandler<IStepper, UIStepper>
	{
		// Trailing glass pill overflow (pts) added to UIStepper width in landscape on iOS 26+.
		// No UIKit API exposes this value; measured empirically on iOS 26.1.
		// If it changes in a future iOS release, update and re-verify. See: https://github.com/dotnet/maui/issues/34273
		const double iOSLiquidGlassStepperOverflow = 20;

		readonly StepperProxy _proxy = new();

		protected override UIStepper CreatePlatformView()
		{
			return new UIStepper(System.Drawing.RectangleF.Empty);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var result = base.GetDesiredSize(widthConstraint, heightConstraint);

			// iOS 26 Liquid Glass workaround: UIStepper renders its glass pill visually beyond
			// its logical frame (Layer.MasksToBounds = false). All UIKit size APIs —
			// IntrinsicContentSize, SizeThatFits, and AlignmentRectInsets — still report the
			// pre-glass logical size (94×32 pts). Apple does not expose the glass overflow extent
			// as a measurable value; this compensation was determined empirically.
			//
			// In landscape orientation the trailing glass overflow is ~20 pts, causing controls
			// adjacent in a HorizontalStackLayout to appear inside the visible glass pill.
			// Portrait orientation has negligible overflow and needs no compensation.
			//
			// If this value changes in a future iOS release, update iOSLiquidGlassStepperOverflow
			// and re-verify on the new OS version. See: https://github.com/dotnet/maui/issues/34273
			// This workaround targets iOS 26+ only; UIStepper on MacCatalyst has not shown the
			// same glass pill overflow behavior in testing. !IsMacCatalyst() is checked first
			// to short-circuit on Mac, since IsIOSVersionAtLeast(26) returns true on both
			// iOS 26+ and Mac Catalyst (macOS 26) — it alone does not distinguish the two.
			if (!OperatingSystem.IsMacCatalyst() && OperatingSystem.IsIOSVersionAtLeast(26))
			{
				var screen = UIKit.UIScreen.MainScreen;
				bool isLandscape = screen.Bounds.Width > screen.Bounds.Height;
				if (isLandscape)
				{
					result = new Size(result.Width + iOSLiquidGlassStepperOverflow, result.Height);
				}
			}

			return result;
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

			// iOS 26+ fix: Adjust stepValue for boundary handling when minimum changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView
				&& NeedsStepValueAdjustment(stepper, platformView))
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		public static void MapMaximum(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateMaximum(stepper);

			// iOS 26+ fix: Adjust stepValue for boundary handling when maximum changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView
				&& NeedsStepValueAdjustment(stepper, platformView))
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		public static void MapIncrement(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateIncrement(stepper);

			// iOS 26+ fix: Adjust stepValue for boundary handling when increment changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView
				&& NeedsStepValueAdjustment(stepper, platformView))
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		public static void MapValue(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateValue(stepper);

			// iOS 26+ fix: Adjust stepValue for boundary handling
			if (OperatingSystem.IsIOSVersionAtLeast(26) && handler.PlatformView is UIStepper platformView
				&& NeedsStepValueAdjustment(stepper, platformView))
			{
				AdjustStepValueForBoundaries(stepper, platformView);
			}
		}

		// Checks whether the step value needs adjustment due to boundary proximity or a previously modified step value.
		static bool NeedsStepValueAdjustment(IStepper stepper, UIStepper platformView)
		{
			const double epsilon = 1e-10;
			return stepper.Value + stepper.Interval > stepper.Maximum
				|| stepper.Value - stepper.Interval < stepper.Minimum
				|| Math.Abs(platformView.StepValue - stepper.Interval) > epsilon;
		}

		// iOS 26+ Workaround: UIStepper behavior changed to prevent button clicks when increment would exceed boundaries
		// instead of clamping to boundary values like previous iOS versions. This method dynamically adjusts
		// the stepValue to match available space to boundaries, allowing users to reach exact min/max values.
		// Reference: https://developer.apple.com/forums/thread/802452
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

		// TODO: Make public for .NET 11.
		internal static void MapFlowDirection(IStepperHandler handler, IStepper stepper)
		{
			handler.PlatformView?.UpdateFlowDirection(stepper);
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
					var oldValue = virtualView.Value;
					var newValue = platformView.Value;

					// iOS 26+ fix: Adjust stepValue for boundary handling
					if (OperatingSystem.IsIOSVersionAtLeast(26)
						&& NeedsStepValueAdjustment(virtualView, platformView))
					{
						AdjustStepValueForBoundaries(virtualView, platformView);
					}

					// iOS 26+ fix: Correct partial steps caused by boundary adjustment.
					// If the step was partial (stepValue was reduced for a boundary) but the
					// full step still fits within [min, max], it was NOT an intentional
					// boundary reach — correct to the full increment.
					if (OperatingSystem.IsIOSVersionAtLeast(26))
					{
						const double epsilon = 1e-10;
						var actualStep = newValue - oldValue;
						var interval = virtualView.Interval;

						if (Math.Abs(actualStep) > epsilon
							&& Math.Abs(Math.Abs(actualStep) - interval) > epsilon)
						{
							var fullStep = oldValue + (actualStep > 0 ? interval : -interval);
							if (fullStep >= virtualView.Minimum && fullStep <= virtualView.Maximum)
							{
								platformView.Value = fullStep;
								platformView.StepValue = interval;
								newValue = fullStep;
							}
						}
					}

					virtualView.Value = newValue;
				}
			}
		}
	}
}