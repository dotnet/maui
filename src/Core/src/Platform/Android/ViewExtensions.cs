using System.Numerics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;
using GL = Android.Opengl;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		const int DefaultAutomationTagId = -1;
		public static int AutomationTagId { get; set; } = DefaultAutomationTagId;

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

		public static ViewStates ToNativeVisibility(this Visibility visibility)
		{
			return visibility switch
			{
				Visibility.Hidden => ViewStates.Invisible,
				Visibility.Collapsed => ViewStates.Gone,
				_ => ViewStates.Visible,
			};
		}

		public static void UpdateBackground(this ContentViewGroup nativeView, IBorder border)
		{
			bool hasBorder = border.Shape != null && border.Stroke != null;

			if (hasBorder)
				nativeView.UpdateMauiDrawable(border);
		}

		public static void UpdateBackground(this AView nativeView, IView view, Drawable? defaultBackground = null)
		{
			// Remove previous background gradient if any
			if (nativeView.Background is MauiDrawable mauiDrawable)
			{
				nativeView.Background = null;
				mauiDrawable.Dispose();
			}

			var paint = view.Background;

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

			if (view.FlowDirection == view.Handler?.MauiContext?.GetFlowDirection() ||
				view.FlowDirection == FlowDirection.MatchParent)
			{
				nativeView.LayoutDirection = ALayoutDirection.Inherit;
			}
			else if (view.FlowDirection == FlowDirection.RightToLeft)
			{
				nativeView.LayoutDirection = ALayoutDirection.Rtl;
			}
			else if (view.FlowDirection == FlowDirection.LeftToRight)
			{
				nativeView.LayoutDirection = ALayoutDirection.Ltr;
			}
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
			if (AutomationTagId == DefaultAutomationTagId)
			{
				AutomationTagId = Resource.Id.automation_tag_id;
			}

			nativeView.SetTag(AutomationTagId, view.AutomationId);
		}

		public static void InvalidateMeasure(this AView nativeView, IView view)
		{
			nativeView.RequestLayout();
		}

		public static void UpdateWidth(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Width into account during the layout
			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void UpdateHeight(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void UpdateMinimumHeight(this AView nativeView, IView view)
		{
			var value = (int)nativeView.Context!.ToPixels(view.MinimumHeight);
			nativeView.SetMinimumHeight(value);

			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void UpdateMinimumWidth(this AView nativeView, IView view)
		{
			var value = (int)nativeView.Context!.ToPixels(view.MinimumWidth);
			nativeView.SetMinimumWidth(value);

			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void UpdateMaximumHeight(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void UpdateMaximumWidth(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void RemoveFromParent(this AView view)
		{
			if (view == null)
				return;

			if (view.Parent == null)
				return;

			((ViewGroup)view.Parent).RemoveView(view);
		}

		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null || nativeView.Context == null)
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
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return new Matrix4x4();
			return nativeView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this View view)
		{
			if (view == null || view.Matrix == null || view.Matrix.IsIdentity)
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
			=> view.GetNative(true).GetBoundingBox();

		internal static Graphics.Rectangle GetBoundingBox(this View? nativeView)
		{
			if (nativeView == null)
				return new Rectangle();

			var rect = new Android.Graphics.Rect();
			nativeView.GetGlobalVisibleRect(rect);
			return new Rectangle(rect.ExactCenterX() - (rect.Width() / 2), rect.ExactCenterY() - (rect.Height() / 2), (float)rect.Width(), (float)rect.Height());
		}
	}
}
