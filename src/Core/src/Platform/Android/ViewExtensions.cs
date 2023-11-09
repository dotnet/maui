using System;
using System.Numerics;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Helper.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.Window.Layout;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using AColor = Android.Graphics.Color;
using ALayoutDirection = Android.Views.LayoutDirection;
using AView = Android.Views.View;
using GL = Android.Opengl;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void Initialize(this AView platformView, IView view)
		{
			var pivotX = (float)(view.AnchorX * platformView.ToPixels(view.Frame.Width));
			var pivotY = (float)(view.AnchorY * platformView.ToPixels(view.Frame.Height));
			int visibility;

			if (view is IActivityIndicator a)
			{
				visibility = (int)a.GetActivityIndicatorVisibility();
			}
			else
			{
				visibility = (int)view.Visibility.ToPlatformVisibility();
			}

			// NOTE: use named arguments for clarity
			PlatformInterop.Set(platformView,
				visibility: visibility,
				layoutDirection: (int)GetLayoutDirection(view),
				minimumHeight: (int)platformView.ToPixels(view.MinimumHeight),
				minimumWidth: (int)platformView.ToPixels(view.MinimumWidth),
				enabled: view.IsEnabled,
				alpha: (float)view.Opacity,
				translationX: platformView.ToPixels(view.TranslationX),
				translationY: platformView.ToPixels(view.TranslationY),
				scaleX: (float)(view.Scale * view.ScaleX),
				scaleY: (float)(view.Scale * view.ScaleY),
				rotation: (float)view.Rotation,
				rotationX: (float)view.RotationX,
				rotationY: (float)view.RotationY,
				pivotX: pivotX,
				pivotY: pivotY
			);
		}

		public static void UpdateIsEnabled(this AView platformView, IView view)
		{
			platformView.Enabled = view.IsEnabled;
		}

		public static void Focus(this AView platformView, FocusRequest request)
		{
			platformView?.Focus(request, null);
		}

		internal static void Focus(this AView platformView, FocusRequest request, Action? focusRequested)
		{
			request.TrySetResult(true);

			// Android does the actual focus/unfocus work on the main looper
			// So in case we're setting the focus in response to another control's un-focusing,
			// we need to post the handling of it to the main looper so that it happens _after_ all the other focus
			// work is done; otherwise, a call to ClearFocus on another control will kill the focus we set 

			var q = Looper.MyLooper();
			if (q != null)
				new Handler(q).Post(RequestFocus);
			else
				MainThread.InvokeOnMainThreadAsync(RequestFocus);

			void RequestFocus()
			{
				if (platformView == null || platformView.IsDisposed())
					return;

				platformView?.RequestFocus();

				if (platformView?.RequestFocus() == true)
					focusRequested?.Invoke();
			}
		}

		public static void Unfocus(this AView platformView, IView view)
		{
			platformView.ClearFocus();
		}

		public static void UpdateVisibility(this AView platformView, IView view)
		{
			platformView.Visibility = view.Visibility.ToPlatformVisibility();
		}

		public static void UpdateClip(this AView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this AView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Shadow = view.Shadow;
		}
		public static void UpdateBorder(this AView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Border = (view as IBorder)?.Border;
		}

		public static ViewStates ToPlatformVisibility(this Visibility visibility)
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
					string? type = context.Resources.GetResourceTypeName(background.ResourceId)?.ToLowerInvariant();

					if (type != null)
					{
						switch (type)
						{
							case "color":
								var color = new AColor(ContextCompat.GetColor(context, background.ResourceId));
								view.SetBackgroundColor(color);
								break;
							case "drawable":
								using (Drawable? drawable = ContextCompat.GetDrawable(context, background.ResourceId))
									view.Background = drawable;
								break;
						}
					}
				}
			}
		}

		public static void UpdateBackground(this ContentViewGroup platformView, IBorderStroke border)
		{
			bool hasBorder = border.Shape != null;

			if (hasBorder)
				platformView.UpdateBorderStroke(border);
		}

		public static void UpdateBackground(this AView platformView, IView view) =>
			platformView.UpdateBackground(view, false);

		internal static void UpdateBackground(this AView platformView, IView view, bool treatTransparentAsNull) =>
			platformView.UpdateBackground(view.Background, treatTransparentAsNull);

		internal static void UpdateBackground(this TextView platformView, IView view) =>
			UpdateBackground(platformView, view, true);

		internal static void UpdateBackground(this EditText platformView, IView view)
		{
			if (platformView is null || platformView.Background is null || platformView.Context is null)
			{
				return;
			}

			// Remove previous background gradient if any
			if (platformView.Background is MauiDrawable mauiDrawable)
			{
				platformView.Background = null;
				mauiDrawable.Dispose();
			}

			if (platformView.Background is LayerDrawable layerDrawable)
			{
				platformView.Background = null;
				layerDrawable.Dispose();
			}

			// Android will reset the padding when setting a Background drawable	
			// So we need to reapply the padding after
			var padLeft = platformView.PaddingLeft;
			var padTop = platformView.PaddingTop;
			var padRight = platformView.PaddingRight;
			var padBottom = platformView.PaddingBottom;

			var paint = view.Background;

			Drawable? defaultBackgroundDrawable = ContextCompat.GetDrawable(platformView.Context, Resource.Drawable.abc_edit_text_material);

			var previousDrawable = defaultBackgroundDrawable ?? platformView.Background;
			var backgroundDrawable = paint.ToDrawable(platformView.Context);

			if (previousDrawable is null)
				platformView.Background = backgroundDrawable;
			else
			{
				if (backgroundDrawable is null)
				{
					// The default Drawable of EditText is an InsetDrawable and setting the background we use a LayerDrawable
					// to compose the custom background with the default behavior (bottom line).
					//
					// If the Background is null or is a ColorDrawable, a Custom Handler is being created, removing the default behavior.
					// In this case, we don't want to reset the Drawable to the default one.
					if (platformView.Background is not ColorDrawable)
						platformView.Background = previousDrawable;
				}
				else
				{

					LayerDrawable layer = new LayerDrawable(new Drawable[] { backgroundDrawable, previousDrawable });
					platformView.Background = layer;
				}
			}

			// Apply previous padding
			platformView.SetPadding(padLeft, padTop, padRight, padBottom);
		}

		public static void UpdateBackground(this AView platformView, Paint? background) =>
			UpdateBackground(platformView, background, false);

		internal static void UpdateBackground(this AView platformView, Paint? background, bool treatTransparentAsNull)
		{
			var paint = background;

			if (!paint.IsNullOrEmpty())
			{
				// Remove previous background gradient if any
				if (platformView.Background is MauiDrawable mauiDrawable)
				{
					platformView.Background = null;
					mauiDrawable.Dispose();
				}

				if (treatTransparentAsNull && paint.IsTransparent())
				{
					// For controls where android treats transparent as null it's more
					// performant to just set the background to null instead of
					// giving it a transparent color/drawable
					platformView.Background = null;
				}
				else if (paint is SolidPaint solidPaint)
				{
					if (solidPaint.Color is Color backgroundColor)
						platformView.SetBackgroundColor(backgroundColor.ToPlatform());
				}
				else
				{
					if (paint!.ToDrawable(platformView.Context) is Drawable drawable)
						platformView.Background = drawable;
				}
			}
			else if (platformView is LayoutViewGroup)
			{
				platformView.Background = null;
			}
		}

		public static void UpdateOpacity(this AView platformView, IView view) => platformView.UpdateOpacity(view.Opacity);

		internal static void UpdateOpacity(this AView platformView, double opacity) => platformView.Alpha = (float)opacity;

		public static void UpdateFlowDirection(this AView platformView, IView view)
		{
			// I realize I could call this method as an extension method
			// But I'm being explicit so if the TextViewExtensions version gets deleted
			// we'll get a compile time exception opposed to an infinite loop
			if (platformView is TextView textview)
			{
				TextViewExtensions.UpdateFlowDirection(textview, view);
				return;
			}

			platformView.LayoutDirection = GetLayoutDirection(view);
		}

		static ALayoutDirection GetLayoutDirection(IView view)
		{
			return view.FlowDirection switch
			{
				FlowDirection.MatchParent => ALayoutDirection.Inherit,
				FlowDirection.LeftToRight => ALayoutDirection.Ltr,
				FlowDirection.RightToLeft => ALayoutDirection.Rtl,
				_ => ALayoutDirection.Inherit,
			};
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

		public static void UpdateAutomationId(this AView platformView, IView view)
		{
			if (!string.IsNullOrWhiteSpace(view.AutomationId))
			{
				PlatformInterop.SetContentDescriptionForAutomationId(platformView, view.AutomationId);
			}
		}

		public static void InvalidateMeasure(this AView platformView, IView view)
		{
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static void UpdateWidth(this AView platformView, IView view)
		{
			// GetDesiredSize will take the specified Width into account during the layout
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static void UpdateHeight(this AView platformView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static void UpdateMinimumHeight(this AView platformView, IView view)
		{
			var min = Dimension.ResolveMinimum(view.MinimumHeight);

			var value = (int)platformView.Context!.ToPixels(min);
			platformView.SetMinimumHeight(value);
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static void UpdateMinimumWidth(this AView platformView, IView view)
		{
			var min = Dimension.ResolveMinimum(view.MinimumWidth);

			var value = (int)platformView.Context!.ToPixels(min);
			platformView.SetMinimumWidth(value);
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static void UpdateMaximumHeight(this AView platformView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static void UpdateMaximumWidth(this AView platformView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			PlatformInterop.RequestLayoutIfNeeded(platformView);
		}

		public static async Task UpdateBackgroundImageSourceAsync(this AView platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			Context? context = platformView.Context;

			if (context == null)
				return;

			if (imageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var result = await service.GetDrawableAsync(imageSource, context);
				Drawable? backgroundImageDrawable = result?.Value;

				if (platformView.IsAlive())
					platformView.Background = backgroundImageDrawable;
			}
		}

		public static void UpdateToolTip(this AView view, ToolTip? tooltip)
		{
			string? text = tooltip?.Content?.ToString();
			TooltipCompat.SetTooltipText(view, text);
		}

		public static void RemoveFromParent(this AView view)
		{
			if (view != null)
				PlatformInterop.RemoveFromParent(view);
		}

		internal static Rect GetPlatformViewBounds(this IView view)
		{
			var platformView = view?.ToPlatform();

			if (platformView?.Context == null)
			{
				return new Rect();
			}

			return platformView.GetPlatformViewBounds();
		}

		internal static Rect GetPlatformViewBounds(this View platformView)
		{
			if (platformView?.Context == null)
				return new Rect();

			var location = new int[2];
			platformView.GetLocationOnScreen(location);
			return new Rect(
				location[0],
				location[1],
				(int)platformView.Context.ToPixels(platformView.Width),
				(int)platformView.Context.ToPixels(platformView.Height));
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return platformView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this View view)
		{
			if (view?.Matrix == null)
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

		internal static Graphics.Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rect GetBoundingBox(this View? platformView)
		{
			if (platformView?.Context == null)
				return new Rect();

			var context = platformView.Context;
			var rect = new Android.Graphics.Rect();
			platformView.GetGlobalVisibleRect(rect);

			return new Rect(
				context.FromPixels(rect.ExactCenterX() - (rect.Width() / 2)),
				context.FromPixels(rect.ExactCenterY() - (rect.Height() / 2)),
				context.FromPixels((float)rect.Width()),
				context.FromPixels((float)rect.Height()));
		}

		internal static bool IsLoaded(this View frameworkElement)
		{
			if (frameworkElement == null)
				return false;

			if (frameworkElement.IsDisposed())
				return false;

			return frameworkElement.IsAttachedToWindow;
		}

		internal static IDisposable OnLoaded(this View view, Action action)
		{
			if (view.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler<AView.ViewAttachedToWindowEventArgs>? routedEventHandler = null;
			ActionDisposable? disposable = new ActionDisposable(() =>
			{
				if (routedEventHandler != null)
					view.ViewAttachedToWindow -= routedEventHandler;
			});

			routedEventHandler = (_, __) =>
			{
				if (!view.IsLoaded() && Looper.MyLooper() is Looper q)
				{
					new Handler(q).Post(() =>
					{
						if (disposable is not null)
							action.Invoke();

						disposable?.Dispose();
						disposable = null;
					});

					return;
				}

				disposable?.Dispose();
				disposable = null;
				action();
			};

			view.ViewAttachedToWindow += routedEventHandler;
			return disposable;
		}

		internal static IDisposable OnUnloaded(this View view, Action action)
		{
			if (!view.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler<AView.ViewDetachedFromWindowEventArgs>? routedEventHandler = null;
			ActionDisposable? disposable = new ActionDisposable(() =>
			{
				if (routedEventHandler != null)
					view.ViewDetachedFromWindow -= routedEventHandler;
			});

			routedEventHandler = (_, __) =>
			{
				// This event seems to fire prior to the view actually being
				// detached from the window
				if (view.IsLoaded() && Looper.MyLooper() is Looper q)
				{
					new Handler(q).Post(() =>
					{
						if (disposable is not null)
							action.Invoke();

						disposable?.Dispose();
						disposable = null;
					});

					return;
				}

				disposable?.Dispose();
				disposable = null;
				action();
			};

			view.ViewDetachedFromWindow += routedEventHandler;
			return disposable;
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
			var destination = Rect.FromLTRB(0, 0,
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

		internal static IWindow? GetHostedWindow(this IView? view)
			=> GetHostedWindow(view?.Handler?.PlatformView as View);

		internal static IWindow? GetHostedWindow(this View? view)
			=> view?.Context?.GetWindow();

		internal static Rect GetFrameRelativeTo(this View view, View relativeTo)
		{
			var viewWindowLocation = view.GetLocationOnScreen();
			var relativeToLocation = relativeTo.GetLocationOnScreen();

			return
				new Rect(
						new Point(viewWindowLocation.X - relativeToLocation.X, viewWindowLocation.Y - relativeToLocation.Y),
						new Graphics.Size(view.Context.FromPixels(view.MeasuredWidth), view.Context.FromPixels(view.MeasuredHeight))
					);
		}

		internal static Rect GetFrameRelativeToWindow(this View view)
		{
			return
				new Rect(view.GetLocationOnScreen(),
				new(view.Context.FromPixels(view.MeasuredHeight), view.Context.FromPixels(view.MeasuredWidth)));
		}

		internal static Point GetLocationOnScreen(this View view)
		{
			int[] location = new int[2];
			view.GetLocationOnScreen(location);
			return new Point(view.Context.FromPixels(location[0]), view.Context.FromPixels(location[1]));
		}

		internal static Point? GetLocationOnScreen(this IElement element)
		{
			if (element.Handler?.MauiContext == null)
				return null;

			return (element.ToPlatform())?.GetLocationOnScreen();
		}

		internal static Point GetLocationOnScreenPx(this View view)
		{
			int[] location = new int[2];
			view.GetLocationOnScreen(location);
			return new Point(location[0], location[1]);
		}

		internal static Point? GetLocationOnScreenPx(this IElement element)
		{
			if (element.Handler?.MauiContext == null)
				return null;

			return (element.ToPlatform())?.GetLocationOnScreenPx();
		}

		internal static bool HideSoftInput(this AView inputView)
		{
			using var inputMethodManager = (InputMethodManager?)inputView.Context?.GetSystemService(Context.InputMethodService);
			var windowToken = inputView.WindowToken;

			if (windowToken is not null && inputMethodManager is not null)
			{
				return inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
			}

			return false;
		}

		internal static bool ShowSoftInput(this TextView inputView)
		{
			using var inputMethodManager = (InputMethodManager?)inputView.Context?.GetSystemService(Context.InputMethodService);

			// The zero value for the second parameter comes from 
			// https://developer.android.com/reference/android/view/inputmethod/InputMethodManager#showSoftInput(android.view.View,%20int)
			// Apparently there's no named value for zero in this case
			return inputMethodManager?.ShowSoftInput(inputView, 0) is true;
		}

		internal static bool ShowSoftInput(this AView view) => view switch
		{
			TextView textView => textView.ShowSoftInput(),
			ViewGroup viewGroup => viewGroup.GetFirstChildOfType<TextView>()?.ShowSoftInput() is true,
			_ => false,
		};

		internal static bool IsSoftInputShowing(this AView view)
		{
			var insets = ViewCompat.GetRootWindowInsets(view);
			if (insets is null)
			{
				return false;
			}

			var result = insets.IsVisible(WindowInsetsCompat.Type.Ime());
			return result;
		}

		internal static void PostShowSoftInput(this AView view)
		{
			void ShowSoftInput()
			{
				// Since we're posting this on the queue, it's possible something else will have disposed of the view
				// by the time the looper is running this, so we have to verify that the view is still usable
				if (view.IsDisposed())
				{
					return;
				}

				view.ShowSoftInput();
			};

			view.Post(ShowSoftInput);
		}
	}
}
