using System.Numerics;
using Microsoft.Maui.Graphics;
using ElmSharp;
using ElmSharp.Accessible;
using Tizen.UIExtensions.ElmSharp;
using static Microsoft.Maui.Primitives.Dimension;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void UpdateIsEnabled(this EvasObject nativeView, IView view)
		{
			if (!(nativeView is Widget widget))
				return;

			widget.IsEnabled = view.IsEnabled;
		}

		public static void UpdateVisibility(this EvasObject nativeView, IView view)
		{
			if (view.Visibility.ToNativeVisibility())
			{
				nativeView.Show();
			}
			else
			{
				nativeView.Hide();
			}
		}

		public static bool ToNativeVisibility(this Visibility visibility)
		{
			return visibility switch
			{
				Visibility.Hidden => false,
				Visibility.Collapsed => false,
				_ => true,
			};
		}

		public static void UpdateBackground(this EvasObject nativeView, IView view)
		{
			if (nativeView is WrapperView wrapperView)
			{
				wrapperView.UpdateBackground(view.Background);
			}
			else if (nativeView is BorderView borderView)
			{
				borderView.ContainerView?.UpdateBackground(view.Background);
			}
			else
			{
				if (view.Background is SolidPaint paint)
				{
					nativeView.UpdateBackgroundColor(paint.Color.ToNative());
				}
			}
		}

		public static void UpdateOpacity(this EvasObject nativeView, IView view)
		{
			if (nativeView is Widget widget)
			{
				widget.Opacity = (int)(view.Opacity * 255.0);
			}
		}

		public static void UpdateClip(this EvasObject nativeView, IView view)
		{
			if (nativeView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this EvasObject nativeView, IView view)
		{
			if (nativeView is WrapperView wrapper)
				wrapper.Shadow = view.Shadow;
		}

		public static void UpdateAutomationId(this EvasObject nativeView, IView view)
		{
			{
				//TODO: EvasObject.AutomationId is supported from tizen60.
				//nativeView.AutomationId = view.AutomationId;
			}
		}

		public static void UpdateSemantics(this EvasObject nativeView, IView view)
		{
			var semantics = view.Semantics;
			var accessibleObject = nativeView as IAccessibleObject;

			if (semantics == null || accessibleObject == null)
				return;

			accessibleObject.Name = semantics.Description;
			accessibleObject.Description = semantics.Hint;
		}

		public static void InvalidateMeasure(this EvasObject nativeView, IView view)
		{
			nativeView.MarkChanged();
		}

		public static void UpdateWidth(this EvasObject nativeView, IView view)
		{
			UpdateSize(nativeView, view);
		}

		public static void UpdateHeight(this EvasObject nativeView, IView view)
		{
			UpdateSize(nativeView, view);
		}

		public static void UpdateMinimumWidth(this EvasObject nativeView, IView view)
		{
			UpdateSize(nativeView, view);
		}

		public static void UpdateMinimumHeight(this EvasObject nativeView, IView view)
		{
			UpdateSize(nativeView, view);
		}

		public static void UpdateMaximumWidth(this EvasObject nativeView, IView view)
		{
			UpdateSize(nativeView, view);
		}

		public static void UpdateMaximumHeight(this EvasObject nativeView, IView view)
		{
			UpdateSize(nativeView, view);
		}

		public static void UpdateSize(EvasObject nativeView, IView view)
		{
			if (!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
			{
				// Ignore the initial setting of the value; the initial layout will take care of it
				return;
			}

			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default (-1) width/height will be taken care of by GetDesiredSize
			nativeView.Resize(view.Width.ToScaledPixel(), view.Height.ToScaledPixel());
		}

		internal static Rectangle GetNativeViewBounds(this IView view)
		{
			var nativeView = view?.GetNative(true);
			if (nativeView == null)
			{
				return new Rectangle();
			}

			return nativeView.GetNativeViewBounds();
		}

		internal static Rectangle GetNativeViewBounds(this EvasObject nativeView)
		{
			if (nativeView == null)
				return new Rectangle();

			return nativeView.Geometry.ToDP();
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var nativeView = view?.GetNative(true);
			if (nativeView == null)
				return new Matrix4x4();
			return nativeView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this EvasObject nativeView)
			=> new Matrix4x4();

		internal static Graphics.Rectangle GetBoundingBox(this IView view)
			=> view.GetNative(true).GetBoundingBox();

		internal static Graphics.Rectangle GetBoundingBox(this EvasObject? nativeView)
		{
			if (nativeView == null)
				return new Rectangle();

			return nativeView.Geometry.ToDP();
		}
	}
}
