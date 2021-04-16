using System.Collections.Generic;
using CoreAnimation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		const string BackgroundLayerName = "MauiBackgroundLayer";

		public static UIColor? GetBackgroundColor(this UIView view)
			=> view?.BackgroundColor;

		public static void UpdateIsEnabled(this UIView nativeView, IView view)
		{
			if (!(nativeView is UIControl uiControl))
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void UpdateBackground(this UIView nativeView, IView view)
		{
			if (nativeView == null)
				return;

			// Remove previous background gradient layer if any
			nativeView.RemoveBackgroundLayer();

			var brush = view.Background;

			if (brush.IsNullOrEmpty())
				return;

			var backgroundLayer = brush?.ToCALayer(nativeView.Bounds);

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
			if (layer == null || layer.Sublayers == null)
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