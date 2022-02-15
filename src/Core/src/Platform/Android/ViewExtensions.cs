using System;
using System.Numerics;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Primitives;
using AColor = Android.Graphics.Color;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;
using GL = Android.Opengl;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void Initialize(this AView nativeView, IView view)
		{
			var context = nativeView.Context;
			if (context == null)
				return;

			var pivotX = (float)(view.AnchorX * context.ToPixels(view.Frame.Width));
			var pivotY = (float)(view.AnchorY * context.ToPixels(view.Frame.Height));
			int visibility;

			if (view is IActivityIndicator a)
			{
				visibility = (int)a.GetActivityIndicatorVisibility();
			}
			else
			{
				visibility = (int)view.Visibility.ToNativeVisibility();
			}

			// NOTE: use named arguments for clarity
			ViewHelper.Set(nativeView,
				visibility: visibility,
				layoutDirection: (int)GetLayoutDirection(view),
				minimumHeight: (int)context.ToPixels(view.MinimumHeight),
				minimumWidth: (int)context.ToPixels(view.MinimumWidth),
				enabled: view.IsEnabled,
				alpha: (float)view.Opacity,
				translationX: context.ToPixels(view.TranslationX),
				translationY: context.ToPixels(view.TranslationY),
				scaleX: (float)(view.Scale * view.ScaleX),
				scaleY: (float)(view.Scale * view.ScaleY),
				rotation: (float)view.Rotation,
				rotationX: (float)view.RotationX,
				rotationY: (float)view.RotationY,
				pivotX: pivotX,
				pivotY: pivotY
			);
		}

		public static void UpdateIsEnabled(this AView nativeView, IView view)
		{
			nativeView.Enabled = view.IsEnabled;
		}

		public static void UpdateVisibility(this AView nativeView, IView view)
		{
			nativeView.Visibility = view.Visibility.ToNativeVisibility();
		}

		public static void UpdateClip(this AView nativeView, IView view)
		{
			if (nativeView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this AView nativeView, IView view)
		{
			if (nativeView is WrapperView wrapper)
				wrapper.Shadow = view.Shadow;
		}
		public static void UpdateBorder(this AView nativeView, IView view)
		{
			if (nativeView is WrapperView wrapper)
				wrapper.Border = (view as IBorder)?.Border;
		}

		public static ViewStates ToNativeVisibility(this Visibility visibility)
		{
			return visibility switch
			{
				Visibility.Hidden => ViewStates.Invisible,
				Visibility.Collapsed => ViewStates.Gone,
				_ => ViewStates.Visible,
			};
		}

		public static void SetWindowBackground(this AView view)
		{
			var context = view.Context;
			if (context?.Theme == null)
				return;

			if (context?.Resources == null)
				return;

			using (var background = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.WindowBackground, background, true))
				{
					string? type = context.Resources.GetResourceTypeName(background.ResourceId)?.ToLower();

					if (type != null)
					{
						switch (type)
						{
							case "color":
								var color = new AColor(ContextCompat.GetColor(context, background.ResourceId));
								view.SetBackgroundColor(color);
								break;
							case "drawable":
								using (Drawable drawable = ContextCompat.GetDrawable(context, background.ResourceId))
									view.Background = drawable;
								break;
						}
					}
				}
			}
		}

		public static void UpdateBackground(this ContentViewGroup nativeView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (hasBorder)
				nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateBackground(this AView nativeView, IView view, Drawable? defaultBackground = null) =>
			nativeView.UpdateBackground(view.Background, defaultBackground);

		public static void UpdateBackground(this AView nativeView, Paint? background, Drawable? defaultBackground = null)
		{
			// Remove previous background gradient if any
			if (nativeView.Background is MauiDrawable mauiDrawable)
			{
				nativeView.Background = null;
				mauiDrawable.Dispose();
			}

			var paint = background;

			if (paint.IsNullOrEmpty())
			{
				if (defaultBackground != null)
					nativeView.Background = defaultBackground;
			}
			else
			{
				if (paint is SolidPaint solidPaint)
				{
					if (solidPaint.Color is Color backgroundColor)
						nativeView.SetBackgroundColor(backgroundColor.ToNative());
				}
				else
				{
					if (paint!.ToDrawable(nativeView.Context) is Drawable drawable)
						nativeView.Background = drawable;
				}
			}
		}

		public static void UpdateOpacity(this AView nativeView, IView view)
		{
			nativeView.Alpha = (float)view.Opacity;
		}

		public static void UpdateFlowDirection(this AView nativeView, IView view)
		{
			// I realize I could call this method as an extension method
			// But I'm being explicit so if the TextViewExtensions version gets deleted
			// we'll get a compile time exception opposed to an infinite loop
			if (nativeView is TextView textview)
			{
				TextViewExtensions.UpdateFlowDirection(textview, view);
				return;
			}

			nativeView.LayoutDirection = GetLayoutDirection(view);
		}

		static ALayoutDirection GetLayoutDirection(IView view)
		{
			if (view.FlowDirection == view.Handler?.MauiContext?.GetFlowDirection() ||
				view.FlowDirection == FlowDirection.MatchParent)
			{
				return ALayoutDirection.Inherit;
			}
			else if (view.FlowDirection == FlowDirection.RightToLeft)
			{
				return ALayoutDirection.Rtl;
			}
			else if (view.FlowDirection == FlowDirection.LeftToRight)
			{
				return ALayoutDirection.Ltr;
			}
			return ALayoutDirection.Inherit;
		}

		public static bool GetClipToOutline(this AView view)
		{
			if (!view.IsAlive())
				return false;

			return view.ClipToOutline;
		}

		public static void SetClipToOutline(this AView view, bool value)
		{
			if (!view.IsAlive())
				return;

			view.ClipToOutline = value;
		}

		public static void UpdateAutomationId(this AView nativeView, IView view)
		{
			if (!string.IsNullOrWhiteSpace(view.AutomationId))
			{
				ViewHelper.SetContentDescriptionForAutomationId(nativeView, view.AutomationId);
			}
		}

		public static void InvalidateMeasure(this AView nativeView, IView view)
		{
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void UpdateWidth(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Width into account during the layout
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void UpdateHeight(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void UpdateMinimumHeight(this AView nativeView, IView view)
		{
			var min = Dimension.ResolveMinimum(view.MinimumHeight);

			var value = (int)nativeView.Context!.ToPixels(min);
			nativeView.SetMinimumHeight(value);
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void UpdateMinimumWidth(this AView nativeView, IView view)
		{
			var min = Dimension.ResolveMinimum(view.MinimumWidth);

			var value = (int)nativeView.Context!.ToPixels(min);
			nativeView.SetMinimumWidth(value);
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void UpdateMaximumHeight(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void UpdateMaximumWidth(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			ViewHelper.RequestLayoutIfNeeded(nativeView);
		}

		public static void RemoveFromParent(this AView view)
		{
			if (view != null)
				ViewHelper.RemoveFromParent(view);
		}

		public static Task<byte[]?> RenderAsPNG(this IView view)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView == null)
				return Task.FromResult<byte[]?>(null);

			return nativeView.RenderAsPNG();
		}

		public static Task<byte[]?> RenderAsJPEG(this IView view)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView == null)
				return Task.FromResult<byte[]?>(null);

			return nativeView.RenderAsJPEG();
		}

		public static Task<byte[]?> RenderAsPNG(this AView view)
			=> Task.FromResult<byte[]?>(view.RenderAsImage(Android.Graphics.Bitmap.CompressFormat.Png));

		public static Task<byte[]?> RenderAsJPEG(this AView view)
			=> Task.FromResult<byte[]?>(view.RenderAsImage(Android.Graphics.Bitmap.CompressFormat.Jpeg));

		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView?.Context == null)
			{
				return new Rectangle();
			}

			return nativeView.GetNativeViewBounds();
		}

		internal static Rectangle GetNativeViewBounds(this View nativeView)
		{
			if (nativeView?.Context == null)
				return new Rectangle();

			var location = new int[2];
			nativeView.GetLocationOnScreen(location);
			return new Rectangle(
				location[0],
				location[1],
				(int)nativeView.Context.ToPixels(nativeView.Width),
				(int)nativeView.Context.ToPixels(nativeView.Height));
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var nativeView = view?.ToPlatform();
			if (nativeView == null)
				return new Matrix4x4();
			return nativeView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this View view)
		{
			if (view?.Matrix == null || view.Matrix.IsIdentity)
				return new Matrix4x4();

			var m = new float[16];
			var v = new float[16];
			var r = new float[16];

			GL.Matrix.SetIdentityM(r, 0);
			GL.Matrix.SetIdentityM(v, 0);
			GL.Matrix.SetIdentityM(m, 0);

			GL.Matrix.TranslateM(v, 0, view.Left, view.Top, 0);
			GL.Matrix.TranslateM(v, 0, view.PivotX, view.PivotY, 0);
			GL.Matrix.TranslateM(v, 0, view.TranslationX, view.TranslationY, 0);
			GL.Matrix.ScaleM(v, 0, view.ScaleX, view.ScaleY, 1);
			GL.Matrix.RotateM(v, 0, view.RotationX, 1, 0, 0);
			GL.Matrix.RotateM(v, 0, view.RotationY, 0, 1, 0);
			GL.Matrix.RotateM(m, 0, view.Rotation, 0, 0, 1);

			GL.Matrix.MultiplyMM(r, 0, v, 0, m, 0);
			GL.Matrix.TranslateM(m, 0, r, 0, -view.PivotX, -view.PivotY, 0);
			return new Matrix4x4
			{
				M11 = m[0],
				M12 = m[1],
				M13 = m[2],
				M14 = m[3],
				M21 = m[4],
				M22 = m[5],
				M23 = m[6],
				M24 = m[7],
				M31 = m[8],
				M32 = m[9],
				M33 = m[10],
				M34 = m[11],
				Translation = new Vector3(m[12], m[13], m[14]),
				M44 = m[15]
			};
		}

		internal static Graphics.Rectangle GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rectangle GetBoundingBox(this View? nativeView)
		{
			if (nativeView == null)
				return new Rectangle();

			var rect = new Android.Graphics.Rect();
			nativeView.GetGlobalVisibleRect(rect);
			return new Rectangle(rect.ExactCenterX() - (rect.Width() / 2), rect.ExactCenterY() - (rect.Height() / 2), (float)rect.Width(), (float)rect.Height());
		}


		internal static void OnLoaded(this View frameworkElement, Action action)
		{
			if (frameworkElement.IsAttachedToWindow)
			{
				action();
			}

			EventHandler<AView.ViewAttachedToWindowEventArgs>? routedEventHandler = null;
			routedEventHandler = (_, __) =>
			{
				if (routedEventHandler != null)
					frameworkElement.ViewAttachedToWindow -= routedEventHandler;

				action();
			};

			frameworkElement.ViewAttachedToWindow += routedEventHandler;
		}

		internal static void OnUnloaded(this View view, Action action)
		{
			if (!view.IsAttachedToWindow)
			{
				action();
			}

			EventHandler<AView.ViewDetachedFromWindowEventArgs>? routedEventHandler = null;
			routedEventHandler = (_, __) =>
			{
				if (routedEventHandler != null)
					view.ViewDetachedFromWindow -= routedEventHandler;

				// This event seems to fire prior to the view actually being
				// detached from the window
				if (view.IsAttachedToWindow)
				{
					var q = Looper.MyLooper();
					if (q != null)
					{
						new Handler(q).Post(action);
						return;
					}
				}

				action();
			};

			view.ViewDetachedFromWindow += routedEventHandler;
		}

		internal static IViewParent? GetParent(this View? view)
		{
			return view?.Parent;
		}

		internal static IViewParent? GetParent(this IViewParent? view)
		{
			return view?.Parent;
		}

		internal static void Arrange(
			this IView view,
			int left,
			int top,
			int right,
			int bottom,
			Context context)
		{
			var deviceIndependentLeft = context.FromPixels(left);
			var deviceIndependentTop = context.FromPixels(top);
			var deviceIndependentRight = context.FromPixels(right);
			var deviceIndependentBottom = context.FromPixels(bottom);
			var destination = Rectangle.FromLTRB(0, 0,
				deviceIndependentRight - deviceIndependentLeft, deviceIndependentBottom - deviceIndependentTop);

			if (!view.Frame.Equals(destination))
				view.Arrange(destination);
		}

		internal static void Arrange(this IView view, AView.LayoutChangeEventArgs e)
		{
			var context = view.Handler?.MauiContext?.Context ??
				 throw new InvalidOperationException("View is Missing Handler");

			view.Arrange(e.Left, e.Top, e.Right, e.Bottom, context);
		}

		internal static void Arrange(this IView view, View platformView)
		{
			var context = platformView.Context ??
				 throw new InvalidOperationException("platformView is Missing Context");

			view.Arrange(
				platformView.Left,
				platformView.Top,
				platformView.Right,
				platformView.Left,
				context);
		}
	}
}
