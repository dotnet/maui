using System;
using ObjCRuntime;
using UIKit;
using CoreGraphics;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this UIStepper platformStepper, IStepper stepper)
		{
			platformStepper.MinimumValue = stepper.Minimum;
		}

		public static void UpdateMaximum(this UIStepper platformStepper, IStepper stepper)
		{
			platformStepper.MaximumValue = stepper.Maximum;
		}

		public static void UpdateIncrement(this UIStepper platformStepper, IStepper stepper)
		{
			var increment = stepper.Interval;

			if (increment > 0)
				platformStepper.StepValue = stepper.Interval;
		}

		public static void UpdateValue(this UIStepper platformStepper, IStepper stepper)
		{
			// Update MinimumValue first to prevent UIStepper from incorrectly clamping the Value.
			// If MAUI updates Value before Minimum, a stale higher MinimumValue would cause iOS to clamp Value incorrectly.
			if (platformStepper.MinimumValue != stepper.Minimum)
			{
				platformStepper.MinimumValue = stepper.Minimum;
			}

			if (platformStepper.Value != stepper.Value)
			{
				platformStepper.Value = stepper.Value;
			}

		}

		// Applies the semantic content attribute and visual transform
		// to the UIStepper and its subviews based on the Stepper's FlowDirection
		// and its parent's layout direction.
		internal static void UpdateFlowDirection(this UIStepper platformStepper, IStepper stepper)
		{
			UISemanticContentAttribute contentAttribute = GetSemanticContentAttribute(stepper);
			// iOS 26 changed UIStepper internal rendering so that SemanticContentAttribute alone
			// is no longer sufficient to mirror the control for RTL. A horizontal transform is required.
			bool isIOS26 = OperatingSystem.IsIOSVersionAtLeast(26);
			CGAffineTransform transform = GetCGAffineTransform(stepper);
			platformStepper.SemanticContentAttribute = contentAttribute;

			if (isIOS26)
			{
				platformStepper.Transform = transform;
			}

			foreach (var subview in platformStepper.Subviews)
			{
				subview.SemanticContentAttribute = contentAttribute;
				// Apply transform to the stepper subviews for 26 version . 
				if (isIOS26)
				{
					subview.Transform = transform;
				}
			}
		}

		static UISemanticContentAttribute GetSemanticContentAttribute(IStepper stepper)
		{
			return stepper.FlowDirection switch
			{
				FlowDirection.LeftToRight => UISemanticContentAttribute.ForceLeftToRight,
				FlowDirection.RightToLeft => UISemanticContentAttribute.ForceRightToLeft,
				_ => GetParentSemanticContentAttribute(stepper),
			};
		}

		static UISemanticContentAttribute GetParentSemanticContentAttribute(IStepper stepper)
		{
			var parentView = (stepper as IView)?.Parent as IView;
			if (parentView is null)
			{
				return UISemanticContentAttribute.Unspecified;
			}

			return parentView.FlowDirection switch
			{
				FlowDirection.LeftToRight => UISemanticContentAttribute.ForceLeftToRight,
				FlowDirection.RightToLeft => UISemanticContentAttribute.ForceRightToLeft,
				_ => UISemanticContentAttribute.Unspecified,
			};
		}

		static CGAffineTransform GetCGAffineTransform(IStepper stepper)
		{
			return stepper.FlowDirection switch
			{
				FlowDirection.LeftToRight => CGAffineTransform.MakeIdentity(),
				FlowDirection.RightToLeft => CGAffineTransform.MakeScale(-1, 1),
				_ => GetParentTransform(stepper), // Default to parent's direction if MatchParent
			};
		}

		static CGAffineTransform GetParentTransform(IStepper stepper)
		{
			var parentSemanticAttribute = GetParentSemanticContentAttribute(stepper);
			if (parentSemanticAttribute == UISemanticContentAttribute.ForceRightToLeft)
			{
				// Flip horizontally for RTL
				return CGAffineTransform.MakeScale(-1, 1);
			}
			// Identity transform for LTR
			return CGAffineTransform.MakeIdentity();
		}
	}
}
