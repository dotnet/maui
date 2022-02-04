using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Platform
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

		public static void UpdateVisibility(this UIView nativeView, IView view) =>
			ViewExtensions.UpdateVisibility(nativeView, view.Visibility);

		public static void UpdateVisibility(this UIView nativeView, Visibility visibility)
		{
			var shouldLayout = false;

			switch (visibility)
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

		public static void UpdateBackground(this ContentView nativeView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (hasBorder)
			{
				nativeView.UpdateMauiCALayer(border);
			}
		}

		public static void UpdateBackground(this UIView nativeView, IView view) =>
			nativeView.UpdateBackground(view.Background);

		public static void UpdateBackground(this UIView nativeView, Paint? paint)
		{
			// Remove previous background gradient layer if any
			nativeView.RemoveBackgroundLayer();

			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				Color backgroundColor = solidPaint.Color;

				if (backgroundColor == null)
					nativeView.BackgroundColor = ColorExtensions.BackgroundColor;
				else
					nativeView.BackgroundColor = backgroundColor.ToPlatform();

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
		public static void UpdateBorder(this UIView nativeView, IView view)
		{
			var border = (view as IBorder)?.Border;
			if (nativeView is WrapperView wrapperView)
				wrapperView.Border = border;
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

		public static Task<byte[]?> RenderAsPNG(this IView view) => view != null ? view.RenderAsImage(true) : Task.FromResult<byte[]?>(null);

		public static Task<byte[]?> RenderAsJPEG(this IView view) => view != null ? view.RenderAsImage(false) : Task.FromResult<byte[]?>(null);

		public static Task<byte[]?> RenderAsPNG(this UIView view, bool skipChildren = true) => view != null ? view.RenderAsImage(skipChildren, true) : Task.FromResult<byte[]?>(null);

		public static Task<byte[]?> RenderAsJPEG(this UIView view, bool skipChildren = true) => view != null ? view.RenderAsImage(skipChildren, false) : Task.FromResult<byte[]?>(null);

		static Task<byte[]?> RenderAsImage(this UIView nativeView, bool skipChildren, bool asPng)
		{
			byte[]? result;
			if (asPng)
				result = nativeView?.Window?.RenderAsPng(nativeView.Layer, UIScreen.MainScreen.Scale, skipChildren);
			else
				result = nativeView?.Window?.RenderAsJpeg(nativeView.Layer, UIScreen.MainScreen.Scale, skipChildren);
			return Task.FromResult<byte[]?>(result);
		}

		static Task<byte[]?> RenderAsImage(this IView view, bool asPng)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView == null)
				return Task.FromResult<byte[]?>(null);
			var skipChildren = !(view is IView && !(view is ILayout));
			return nativeView.RenderAsImage(skipChildren, asPng);
		}

		internal static Rectangle GetPlatformViewBounds(this IView view)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView == null)
			{
				return new Rectangle();
			}

			return nativeView.GetPlatformViewBounds();
		}

		internal static Rectangle GetPlatformViewBounds(this UIView nativeView)
		{
			if (nativeView == null)
				return new Rectangle();

			var superview = nativeView;
			while (superview.Superview is not null)
			{
				superview = superview.Superview;
			}

			var convertPoint = nativeView.ConvertRectToView(nativeView.Bounds, superview);

			var X = convertPoint.X;
			var Y = convertPoint.Y;
			var Width = convertPoint.Width;
			var Height = convertPoint.Height;

			return new Rectangle(X, Y, Width, Height);
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView == null)
				return new Matrix4x4();
			return nativeView.Layer.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this UIView view)
			=> view.Layer.GetViewTransform();

		internal static Graphics.Rectangle GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rectangle GetBoundingBox(this UIView? nativeView)
		{
			if (nativeView == null)
				return new Rectangle();
			var nvb = nativeView.GetPlatformViewBounds();
			var transform = nativeView.GetViewTransform();
			var radians = transform.ExtractAngleInRadians();
			var rotation = CoreGraphics.CGAffineTransform.MakeRotation((nfloat)radians);
			CGAffineTransform.CGRectApplyAffineTransform(nvb, rotation);
			return new Rectangle(nvb.X, nvb.Y, nvb.Width, nvb.Height);
		}

		internal static UIView? GetParent(this UIView? view)
		{
			return view?.Superview;
		}
	}
}