using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		const string BackgroundLayerName = "MauiBackgroundLayer";

		public static void UpdateIsEnabled(this UIView nativeView, IView view)
		{
			if (nativeView is not UIControl uiControl)
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void UpdateVisibility(this UIView nativeView, IView view)
		{
			var shouldLayout = false;

			switch (view.Visibility)
			{
				case Visibility.Visible:
					shouldLayout = nativeView.Inflate();
					nativeView.Hidden = false;
					break;
				case Visibility.Hidden:
					shouldLayout = nativeView.Inflate();
					nativeView.Hidden = true;
					break;
				case Visibility.Collapsed:
					nativeView.Hidden = true;
					nativeView.Collapse();
					shouldLayout = true;
					break;
			}

			// If the view is just switching between Visible and Hidden, then a re-layout isn't necessary. The return value
			// from Inflate will tell us if the view was previously collapsed. If the view is switching to or from a collapsed
			// state, then we'll have to ask for a re-layout.

			if (shouldLayout)
			{
				nativeView.Superview?.SetNeedsLayout();
			}
		}

		public static void UpdateBackground(this UIView nativeView, IView view)
		{
			if (nativeView == null)
				return;

			// Remove previous background gradient layer if any
			nativeView.RemoveBackgroundLayer();

			var paint = view.Background;

			if (paint.IsNullOrEmpty())
				return;

			var backgroundLayer = paint?.ToCALayer(nativeView.Bounds);

			if (backgroundLayer != null)
			{
				backgroundLayer.Name = BackgroundLayerName;
				nativeView.BackgroundColor = UIColor.Clear;
				nativeView.InsertBackgroundLayer(backgroundLayer, 0);
			}
		}

		public static void UpdateAutomationId(this UIView nativeView, IView view) =>
			nativeView.AccessibilityIdentifier = view.AutomationId;

		public static void UpdateSemantics(this UIView nativeView, IView view)
		{
			var semantics = view.Semantics;

			if (semantics == null)
				return;

			nativeView.AccessibilityLabel = semantics.Description;
			nativeView.AccessibilityHint = semantics.Hint;

			if (semantics.IsHeading)
				nativeView.AccessibilityTraits |= UIAccessibilityTrait.Header;
			else
				nativeView.AccessibilityTraits &= ~UIAccessibilityTrait.Header;
		}

		public static T? FindDescendantView<T>(this UIView view) where T : UIView
		{
			var queue = new Queue<UIView>();
			queue.Enqueue(view);

			while (queue.Count > 0)
			{
				var descendantView = queue.Dequeue();

				if (descendantView is T result)
					return result;

				for (var i = 0; i < descendantView.Subviews?.Length; i++)
					queue.Enqueue(descendantView.Subviews[i]);
			}

			return null;
		}

		public static void UpdateBackgroundLayerFrame(this UIView view)
		{
			if (view == null || view.Frame.IsEmpty)
				return;

			var layer = view.Layer;

			if (layer == null || layer.Sublayers == null || layer.Sublayers.Length == 0)
				return;

			foreach (var sublayer in layer.Sublayers)
			{
				if (sublayer.Name == BackgroundLayerName && sublayer.Frame != view.Bounds)
				{
					sublayer.Frame = view.Bounds;
					break;
				}
			}
		}

		public static void InvalidateMeasure(this UIView nativeView, IView view)
		{
			nativeView.SetNeedsLayout();
		}

		public static void UpdateWidth(this UIView nativeView, IView view)
		{
			if (view.Width == -1)
			{
				// Ignore the initial set of the height; the initial layout will take care of it
				return;
			}

			UpdateFrame(nativeView, view);
		}

		public static void UpdateHeight(this UIView nativeView, IView view)
		{
			if (view.Height == -1)
			{
				// Ignore the initial set of the height; the initial layout will take care of it
				return;
			}

			UpdateFrame(nativeView, view);
		}

		public static void UpdateFrame(UIView nativeView, IView view)
		{
			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default (-1) width/height will be taken care of by GetDesiredSize
			var currentFrame = nativeView.Frame;
			nativeView.Frame = new CoreGraphics.CGRect(currentFrame.X, currentFrame.Y, view.Width, view.Height);
		}

		public static int IndexOfSubview(this UIView nativeView, UIView subview)
		{
			if (nativeView.Subviews.Length == 0)
				return -1;

			return Array.IndexOf(nativeView.Subviews, subview);
		}

		internal static void Collapse(this UIView view)
		{
			// See if this view already has a collapse constraint we can use
			foreach (var constraint in view.Constraints)
			{
				if (constraint is CollapseConstraint collapseConstraint)
				{
					// Active the collapse constraint; that will squish the view down to zero height
					collapseConstraint.Active = true;
					return;
				}
			}

			// Set up a collapse constraint and turn it on
			var collapse = new CollapseConstraint();
			view.AddConstraint(collapse);
			collapse.Active = true;
		}

		internal static bool Inflate(this UIView view)
		{
			// Find and deactivate the collapse constraint, if any; the view will go back to its normal height
			foreach (var constraint in view.Constraints)
			{
				if (constraint is CollapseConstraint collapseConstraint)
				{
					collapseConstraint.Active = false;
					return true;
				}
			}

			return false;
		}

		static void InsertBackgroundLayer(this UIView control, CALayer backgroundLayer, int index = -1)
		{
			control.RemoveBackgroundLayer();

			if (backgroundLayer != null)
			{
				var layer = control.Layer;

				if (index > -1)
					layer.InsertSublayer(backgroundLayer, index);
				else
					layer.AddSublayer(backgroundLayer);
			}
		}

		static void RemoveBackgroundLayer(this UIView control)
		{
			var layer = control.Layer;

			if (layer == null)
				return;

			if (layer.Name == BackgroundLayerName)
			{
				layer.RemoveFromSuperLayer();
				return;
			}

			if (layer.Sublayers == null || layer.Sublayers.Length == 0)
				return;

			foreach (var subLayer in layer.Sublayers)
			{
				if (subLayer.Name == BackgroundLayerName)
				{
					subLayer.RemoveFromSuperLayer();
					break;
				}
			}
		}
	}
}