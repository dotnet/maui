using System;
using System.Collections.Generic;
using System.Numerics;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui
{
	public static partial class ViewExtensions
	{
		internal const string BackgroundLayerName = "MauiBackgroundLayer";

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

		public static void UpdateBackground(this ContentView nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (hasBorder)
			{
				nativeView.UpdateMauiCALayer(border);
			}
		}

		public static void UpdateBackground(this UIView nativeView, IView view)
		{
			// Remove previous background gradient layer if any
			nativeView.RemoveBackgroundLayer();

			var paint = view.Background;

			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				Color backgroundColor = solidPaint.Color;

				if (backgroundColor == null)
					nativeView.BackgroundColor = ColorExtensions.BackgroundColor;
				else
					nativeView.BackgroundColor = backgroundColor.ToNative();

				return;
			}
			else if (paint is GradientPaint gradientPaint)
			{
				var backgroundLayer = gradientPaint?.ToCALayer(nativeView.Bounds);

				if (backgroundLayer != null)
				{
					backgroundLayer.Name = BackgroundLayerName;
					nativeView.BackgroundColor = UIColor.Clear;
					nativeView.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}
		}

		public static void UpdateFlowDirection(this UIView nativeView, IView view)
		{
			UISemanticContentAttribute updateValue = nativeView.SemanticContentAttribute;

			if (view.FlowDirection == view.Handler?.MauiContext?.GetFlowDirection() ||
				view.FlowDirection == FlowDirection.MatchParent)
			{
				updateValue = UISemanticContentAttribute.Unspecified;
			}
			else if (view.FlowDirection == FlowDirection.RightToLeft)
				updateValue = UISemanticContentAttribute.ForceRightToLeft;
			else if (view.FlowDirection == FlowDirection.LeftToRight)
				updateValue = UISemanticContentAttribute.ForceLeftToRight;

			if (updateValue != nativeView.SemanticContentAttribute)
				nativeView.SemanticContentAttribute = updateValue;
		}

		public static void UpdateOpacity(this UIView nativeView, IView view)
		{
			nativeView.Alpha = (float)view.Opacity;
		}

		public static void UpdateAutomationId(this UIView nativeView, IView view) =>
			nativeView.AccessibilityIdentifier = view.AutomationId;

		public static void UpdateClip(this UIView nativeView, IView view)
		{
			if (nativeView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this UIView nativeView, IView view)
		{
			var shadow = view.Shadow;
			var clip = view.Clip;

			// If there is a clip shape, then the shadow should be applied to the clip layer, not the view layer
			if (clip == null)
			{
				if (shadow == null)
					nativeView.ClearShadow();
				else
					nativeView.SetShadow(shadow);
			}
			else
			{
				if (nativeView is WrapperView wrapperView)
					wrapperView.Shadow = view.Shadow;
			}
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
			nativeView.Superview?.SetNeedsLayout();
		}

		public static void UpdateWidth(this UIView nativeView, IView view)
		{
			UpdateFrame(nativeView, view);
		}

		public static void UpdateHeight(this UIView nativeView, IView view)
		{
			UpdateFrame(nativeView, view);
		}

		public static void UpdateMinimumHeight(this UIView nativeView, IView view)
		{
			UpdateFrame(nativeView, view);
		}

		public static void UpdateMaximumHeight(this UIView nativeView, IView view)
		{
			UpdateFrame(nativeView, view);
		}

		public static void UpdateMinimumWidth(this UIView nativeView, IView view)
		{
			UpdateFrame(nativeView, view);
		}

		public static void UpdateMaximumWidth(this UIView nativeView, IView view)
		{
			UpdateFrame(nativeView, view);
		}

		public static void UpdateFrame(UIView nativeView, IView view)
		{
			if (!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
			{
				// Ignore the initial setting of the value; the initial layout will take care of it
				return;
			}

			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default width/height will be taken care of by GetDesiredSize
			var currentFrame = nativeView.Frame;
			nativeView.Frame = new CoreGraphics.CGRect(currentFrame.X, currentFrame.Y, view.Width, view.Height);
		}

		public static int IndexOfSubview(this UIView nativeView, UIView subview)
		{
			if (nativeView.Subviews.Length == 0)
				return -1;

			return Array.IndexOf(nativeView.Subviews, subview);
		}

		public static UIImage? ConvertToImage(this UIView view)
		{
			if (!NativeVersion.IsAtLeast(10))
			{
				UIGraphics.BeginImageContext(view.Frame.Size);
				view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
				var image = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();

				if (image.CGImage == null)
					return null;

				return new UIImage(image.CGImage);
			}

			var imageRenderer = new UIGraphicsImageRenderer(view.Bounds.Size);

			return imageRenderer.CreateImage((a) =>
			{
				view.Layer.RenderInContext(a.CGContext);
			});
		}

		public static UINavigationController? GetNavigationController(this UIView view)
		{
			var rootController = view.Window?.RootViewController;
			if (rootController is UINavigationController nc)
				return nc;

			return rootController?.NavigationController;
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

		public static void ClearSubviews(this UIView view)
		{
			for (int n = view.Subviews.Length - 1; n >= 0; n--)
			{
				view.Subviews[n].RemoveFromSuperview();
			}
		}

		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
			{
				return new Rectangle();
			}

			var uiWindow = nativeView.GetUIWindow();
			if (uiWindow == null)
				return new Rectangle();

			nfloat X;
			nfloat Y;
			nfloat Width;
			nfloat Height;

			var convertPoint = nativeView.ConvertRectToView(nativeView.Bounds, uiWindow);

			X = convertPoint.X;
			Y = convertPoint.Y;
			Width = convertPoint.Width;
			Height = convertPoint.Height;

			return new Rectangle(X, Y, Width, Height);
		}

		internal static CGRect GetNativeViewBounds(this UIView nativeView)
		{
			var uiWindow = nativeView.GetUIWindow();
			if (uiWindow == null)
				return new CGRect();

			return nativeView.ConvertRectToView(nativeView.Bounds, uiWindow);
		}


		internal static UIKit.UIWindow? GetUIWindow(this UIKit.UIView view)
		{
			if (view is UIKit.UIWindow window)
				return window;

			if (view.Superview != null)
				return GetUIWindow(view.Superview);

			return null;
		}

		internal static Matrix4x4 ToViewTransform(this CATransform3D transform) =>
			new Matrix4x4
			{
		M11 = (float)transform.m11,
		M12 = (float)transform.m12,
		M13 = (float)transform.m13,
		M14 = (float)transform.m14,
		M21 = (float)transform.m21,
		M22 = (float)transform.m22,
		M23 = (float)transform.m23,
		M24 = (float)transform.m24,
		M31 = (float)transform.m31,
		M32 = (float)transform.m32,
		M33 = (float)transform.m33,
		M34 = (float)transform.m34,
		Translation = new Vector3((float)transform.m41, (float)transform.m42, (float)transform.m43),
		M44 = (float)transform.m44
			};

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return new Matrix4x4();
			return GetViewTransform(nativeView.Layer);
		}

		internal static Matrix4x4 GetViewTransform(CALayer layer)
		{
			if (layer == null)
				return new Matrix4x4();

			var superLayer = layer.SuperLayer;
			if (layer.Transform.IsIdentity && (superLayer == null || superLayer.Transform.IsIdentity))
				return new Matrix4x4();

			var superTransform = layer.SuperLayer?.GetChildTransform() ?? CATransform3D.Identity;

			return layer.GetLocalTransform()
				.Concat(superTransform)
					.ToViewTransform();
		}

		internal static Graphics.Rectangle GetBoundingBox(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return new Rectangle();
			var nvb = nativeView.GetNativeViewBounds();
			var transform = view.GetViewTransform();
			var radians = transform.ExtractAngleInRadians();
			var rotation = CoreGraphics.CGAffineTransform.MakeRotation((nfloat)radians);
			CGAffineTransform.CGRectApplyAffineTransform(nvb, rotation);
			return new Rectangle(nvb.X, nvb.Y, nvb.Width, nvb.Height);
		}
	}

	public static class CoreAnimationExtensions
	{
		internal static CATransform3D Prepend(this CATransform3D a, CATransform3D b) =>
			b.Concat(a);

		internal static CATransform3D GetLocalTransform(this CALayer layer)
		{
			return CATransform3D.Identity
				.Translate(
					layer.Position.X,
					layer.Position.Y,
					layer.ZPosition)
				.Prepend(layer.Transform)
				.Translate(
					-layer.AnchorPoint.X * layer.Bounds.Width,
					-layer.AnchorPoint.Y * layer.Bounds.Height,
					-layer.AnchorPointZ);
		}

		internal static CATransform3D GetChildTransform(this CALayer layer)
		{
			var childTransform = layer.SublayerTransform;

			if (childTransform.IsIdentity)
				return childTransform;

			return CATransform3D.Identity
				.Translate(
					layer.AnchorPoint.X * layer.Bounds.Width,
					layer.AnchorPoint.Y * layer.Bounds.Height,
					layer.AnchorPointZ)
				.Prepend(childTransform)
				.Translate(
					-layer.AnchorPoint.X * layer.Bounds.Width,
					-layer.AnchorPoint.Y * layer.Bounds.Height,
					-layer.AnchorPointZ);
		}

		internal static CATransform3D TransformToAncestor(this CALayer fromLayer, CALayer toLayer)
		{
			var transform = CATransform3D.Identity;

			CALayer? current = fromLayer;
			while (current != toLayer)
			{
				transform = transform.Concat(current.GetLocalTransform());

				current = current.SuperLayer;
				if (current == null)
					break;

				transform = transform.Concat(current.GetChildTransform());
			}
			return transform;
		}
	}
}
