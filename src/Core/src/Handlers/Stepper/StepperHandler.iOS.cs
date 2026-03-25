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