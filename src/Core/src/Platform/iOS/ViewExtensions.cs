using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		internal const string BackgroundLayerName = "MauiBackgroundLayer";

		public static void UpdateIsEnabled(this UIView platformView, IView view)
		{
			if (platformView is not UIControl uiControl)
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void Focus(this UIView platformView, FocusRequest request)
		{
			platformView.BecomeFirstResponder();
		}

		public static void Unfocus(this UIView platformView, IView view)
		{
			platformView.ResignFirstResponder();
		}

		public static void UpdateVisibility(this UIView platformView, IView view) =>
			ViewExtensions.UpdateVisibility(platformView, view.Visibility);

		public static void UpdateVisibility(this UIView platformView, Visibility visibility)
		{
			var shouldLayout = false;

			switch (visibility)
			{
				case Visibility.Visible:
					shouldLayout = platformView.Inflate();
					platformView.Hidden = false;
					break;
				case Visibility.Hidden:
					shouldLayout = platformView.Inflate();
					platformView.Hidden = true;
					break;
				case Visibility.Collapsed:
					platformView.Hidden = true;
					platformView.Collapse();
					shouldLayout = true;
					break;
			}

			// If the view is just switching between Visible and Hidden, then a re-layout isn't necessary. The return value
			// from Inflate will tell us if the view was previously collapsed. If the view is switching to or from a collapsed
			// state, then we'll have to ask for a re-layout.

			if (shouldLayout)
			{
				platformView.Superview?.SetNeedsLayout();
			}
		}

		public static void UpdateBackground(this ContentView platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (hasBorder)
			{
				platformView.UpdateMauiCALayer(border);
			}
		}

		public static void UpdateBackground(this UIView platformView, IView view) =>
			platformView.UpdateBackground(view.Background);

		public static void UpdateBackground(this UIView platformView, Paint? paint)
		{
			// Remove previous background gradient layer if any
			platformView.RemoveBackgroundLayer();

			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				Color backgroundColor = solidPaint.Color;

				if (backgroundColor == null)
					platformView.BackgroundColor = ColorExtensions.BackgroundColor;
				else
					platformView.BackgroundColor = backgroundColor.ToPlatform();

				return;
			}
			else if (paint is GradientPaint gradientPaint)
			{
				var backgroundLayer = gradientPaint?.ToCALayer(platformView.Bounds);

				if (backgroundLayer != null)
				{
					backgroundLayer.Name = BackgroundLayerName;
					platformView.BackgroundColor = UIColor.Clear;
					platformView.InsertBackgroundLayer(backgroundLayer, 0);
				}
			}
		}

		public static void UpdateFlowDirection(this UIView platformView, IView view)
		{
			UISemanticContentAttribute updateValue = platformView.SemanticContentAttribute;

			if (view.FlowDirection == view.Handler?.MauiContext?.GetFlowDirection() ||
				view.FlowDirection == FlowDirection.MatchParent)
			{
				updateValue = UISemanticContentAttribute.Unspecified;
			}
			else if (view.FlowDirection == FlowDirection.RightToLeft)
				updateValue = UISemanticContentAttribute.ForceRightToLeft;
			else if (view.FlowDirection == FlowDirection.LeftToRight)
				updateValue = UISemanticContentAttribute.ForceLeftToRight;

			if (updateValue != platformView.SemanticContentAttribute)
				platformView.SemanticContentAttribute = updateValue;
		}

		public static void UpdateOpacity(this UIView platformView, IView view)
		{
			platformView.Alpha = (float)view.Opacity;
		}

		public static void UpdateAutomationId(this UIView platformView, IView view) =>
			platformView.AccessibilityIdentifier = view.AutomationId;

		public static void UpdateClip(this UIView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this UIView platformView, IView view)
		{
			var shadow = view.Shadow;
			var clip = view.Clip;

			// If there is a clip shape, then the shadow should be applied to the clip layer, not the view layer
			if (clip == null)
			{
				if (shadow == null)
					platformView.ClearShadow();
				else
					platformView.SetShadow(shadow);
			}
			else
			{
				if (platformView is WrapperView wrapperView)
					wrapperView.Shadow = view.Shadow;
			}
		}
		public static void UpdateBorder(this UIView platformView, IView view)
		{
			var border = (view as IBorder)?.Border;
			if (platformView is WrapperView wrapperView)
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

		public static void InvalidateMeasure(this UIView platformView, IView view)
		{
			platformView.SetNeedsLayout();
			platformView.Superview?.SetNeedsLayout();
		}

		public static void UpdateWidth(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateHeight(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMinimumHeight(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMaximumHeight(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMinimumWidth(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateMaximumWidth(this UIView platformView, IView view)
		{
			UpdateFrame(platformView, view);
		}

		public static void UpdateFrame(UIView platformView, IView view)
		{
			if (!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
			{
				// Ignore the initial setting of the value; the initial layout will take care of it
				return;
			}

			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default width/height will be taken care of by GetDesiredSize
			var currentFrame = platformView.Frame;
			platformView.Frame = new CoreGraphics.CGRect(currentFrame.X, currentFrame.Y, view.Width, view.Height);
		}

		public static int IndexOfSubview(this UIView platformView, UIView subview)
		{
			if (platformView.Subviews.Length == 0)
				return -1;

			return Array.IndexOf(platformView.Subviews, subview);
		}

		public static UIImage? ConvertToImage(this UIView view)
		{
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

		static Task<byte[]?> RenderAsImage(this UIView platformView, bool skipChildren, bool asPng)
		{
			byte[]? result;
			if (asPng)
				result = platformView?.Window?.RenderAsPng(platformView.Layer, UIScreen.MainScreen.Scale, skipChildren);
			else
				result = platformView?.Window?.RenderAsJpeg(platformView.Layer, UIScreen.MainScreen.Scale, skipChildren);
			return Task.FromResult<byte[]?>(result);
		}

		static Task<byte[]?> RenderAsImage(this IView view, bool asPng)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return Task.FromResult<byte[]?>(null);
			var skipChildren = !(view is IView && !(view is ILayout));
			return platformView.RenderAsImage(skipChildren, asPng);
		}

		internal static Rect GetPlatformViewBounds(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
			{
				return new Rect();
			}

			return platformView.GetPlatformViewBounds();
		}

		internal static Rect GetPlatformViewBounds(this UIView platformView)
		{
			if (platformView == null)
				return new Rect();

			var superview = platformView;
			while (superview.Superview is not null)
			{
				superview = superview.Superview;
			}

			var convertPoint = platformView.ConvertRectToView(platformView.Bounds, superview);

			var X = convertPoint.X;
			var Y = convertPoint.Y;
			var Width = convertPoint.Width;
			var Height = convertPoint.Height;

			return new Rect(X, Y, Width, Height);
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return platformView.Layer.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this UIView view)
			=> view.Layer.GetViewTransform();

		internal static Graphics.Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rect GetBoundingBox(this UIView? platformView)
		{
			if (platformView == null)
				return new Rect();
			var nvb = platformView.GetPlatformViewBounds();
			var transform = platformView.GetViewTransform();
			var radians = transform.ExtractAngleInRadians();
			var rotation = CoreGraphics.CGAffineTransform.MakeRotation((nfloat)radians);
			CGAffineTransform.CGRectApplyAffineTransform(nvb, rotation);
			return new Rect(nvb.X, nvb.Y, nvb.Width, nvb.Height);
		}

		internal static UIView? GetParent(this UIView? view)
		{
			return view?.Superview;
		}

		internal static void LayoutToSize(this IView view, double width, double height)
		{
			var platformFrame = new CGRect(0, 0, width, height);

			if (view.Handler is IPlatformViewHandler viewHandler && viewHandler.PlatformView != null)
				viewHandler.PlatformView.Frame = platformFrame;

			view.Arrange(platformFrame.ToRectangle());
		}

		internal static Size LayoutToMeasuredSize(this IView view, double width, double height)
		{
			var size = view.Measure(width, height);
			var platformFrame = new CGRect(0, 0, size.Width, size.Height);

			if (view.Handler is IPlatformViewHandler viewHandler && viewHandler.PlatformView != null)
				viewHandler.PlatformView.Frame = platformFrame;

			view.Arrange(platformFrame.ToRectangle());
			return size;
		}
		
		public static void UpdateInputTransparent(this UIView platformView, IViewHandler handler, IView view)
		{
			if (view is ITextInput textInput)
			{
				platformView.UpdateInputTransparent(textInput.IsReadOnly, view.InputTransparent);
				return;
			}

			platformView.UserInteractionEnabled = !view.InputTransparent;
		}

		public static void UpdateInputTransparent(this UIView platformView, bool isReadOnly, bool inputTransparent) 
		{
			platformView.UserInteractionEnabled = !(isReadOnly || inputTransparent);
		}

		internal static IWindow? GetHostedWindow(this IView? view)
			=> GetHostedWindow(view?.Handler?.PlatformView as UIView);

		internal static IWindow? GetHostedWindow(this UIView? view)
			=> GetHostedWindow(view?.Window);

		internal static bool IsLoaded(this UIView uiView) =>
			uiView.Window != null;

		internal static IDisposable OnLoaded(this UIView uiView, Action action)
		{
			if (uiView.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			Dictionary<NSString, NSObject> observers = new Dictionary<NSString, NSObject>();
			ActionDisposable? disposable = new ActionDisposable(() =>
			{
				foreach (var thing in observers)
					uiView.Layer.RemoveObserver(thing.Value, thing.Key);
			});

			// Ideally we could wire into UIView.MovedToWindow but there's no way to do that without just inheriting from every single
			// UIView. So we just make our best attempt by observering some properties that are going to fire once UIView is attached to a window.			
			observers.Add(new NSString("bounds"), (NSObject)uiView.Layer.AddObserver("bounds", Foundation.NSKeyValueObservingOptions.OldNew, (_) => OnLoadedCheck()));
			observers.Add(new NSString("frame"), (NSObject)uiView.Layer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.OldNew, (_) => OnLoadedCheck()));

			// OnLoaded is called at the point in time where the xplat view knows it's going to be attached to the window.
			// So this just serves as a way to queue a call on the UI Thread to see if that's enough time for the window
			// to get attached.
			uiView.BeginInvokeOnMainThread(OnLoadedCheck);

			void OnLoadedCheck()
			{
				if (uiView.IsLoaded() && disposable != null)
				{
					disposable.Dispose();
					disposable = null;
					action();
				}
			};

			return disposable;
		}

		internal static IDisposable OnUnloaded(this UIView uiView, Action action)
		{

			if (!uiView.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			Dictionary<NSString, NSObject> observers = new Dictionary<NSString, NSObject>();
			ActionDisposable? disposable = new ActionDisposable(() =>
			{
				foreach (var thing in observers)
					uiView.Layer.RemoveObserver(thing.Value, thing.Key);
			});

			// Ideally we could wire into UIView.MovedToWindow but there's no way to do that without just inheriting from every single
			// UIView. So we just make our best attempt by observering some properties that are going to fire once UIView is attached to a window.	
			observers.Add(new NSString("bounds"), (NSObject)uiView.Layer.AddObserver("bounds", Foundation.NSKeyValueObservingOptions.OldNew, (_) => UnLoadedCheck()));
			observers.Add(new NSString("frame"), (NSObject)uiView.Layer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.OldNew, (_) => UnLoadedCheck()));

			// OnUnloaded is called at the point in time where the xplat view knows it's going to be detached from the window.
			// So this just serves as a way to queue a call on the UI Thread to see if that's enough time for the window
			// to get detached.
			uiView.BeginInvokeOnMainThread(UnLoadedCheck);

			void UnLoadedCheck()
			{
				if (!uiView.IsLoaded() && disposable != null)
				{
					disposable.Dispose();
					disposable = null;
					action();
				}
			};

			return disposable;
		}
	}
}
