using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using static Microsoft.Maui.Primitives.Dimension;
using NColor = Tizen.NUI.Color;
using NView = Tizen.NUI.BaseComponents.View;
using Rect = Microsoft.Maui.Graphics.Rect;
using TRect = Tizen.UIExtensions.Common.Rect;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void UpdateIsEnabled(this NView platformView, IView view)
		{
			platformView.IsEnabled = view.IsEnabled;
		}

		public static void Focus(this NView platformView, FocusRequest request)
		{
			request.TrySetResult(Tizen.NUI.FocusManager.Instance.SetCurrentFocusView(platformView));
		}

		public static void Unfocus(this NView platformView, IView view)
		{
			if (Tizen.NUI.FocusManager.Instance.GetCurrentFocusView() == platformView)
				Tizen.NUI.FocusManager.Instance.ClearFocus();
		}

		public static void UpdateVisibility(this NView platformView, IView view)
		{
			if (view.Visibility.ToPlatformVisibility())
			{
				platformView.Show();
				platformView.Layout?.RequestLayout();
			}
			else
			{
				platformView.Hide();
			}
		}

		public static bool ToPlatformVisibility(this Visibility visibility)
		{
			return visibility switch
			{
				Visibility.Hidden => false,
				Visibility.Collapsed => false,
				_ => true,
			};
		}

		public static void UpdateBackground(this ContentViewGroup platformView, IBorderView border)
		{
			(platformView.GetParent() as WrapperView)?.UpdateBackground(border.Background);
		}

		public static void UpdateBackground(this NView platformView, IView view)
		{
			if (view.Background is ImageSourcePaint image)
			{
				var provider = view.Handler?.GetRequiredService<IImageSourceServiceProvider>();
				platformView.UpdateBackgroundImageSourceAsync(image.ImageSource, provider)
					.FireAndForget();
				return;
			}

			var paint = view.Background;

			if (platformView is WrapperView wrapperView)
			{
				wrapperView.UpdateBackground(paint);
			}
			else if (platformView.GetParent() is WrapperView parent)
			{
				parent.UpdateBackground(paint);
				platformView.BackgroundColor = NColor.Transparent;
			}
			else if (paint is not null)
			{
				platformView.UpdateBackgroundColor(paint.ToPlatform());
			}
		}

		public static void UpdateBackground(this NView platformView, Paint? paint)
		{
			if (paint == null)
				return;

			if (platformView is WrapperView wrapperView)
			{
				wrapperView.UpdateBackground(paint);
			}
			else if (paint is not null)
			{
				platformView.UpdateBackgroundColor(paint.ToPlatform());
			}
		}

		public static async Task UpdateBackgroundImageSourceAsync(this NView platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			if (platformView is WrapperView wrapperView && wrapperView.Content != null)
			{
				await UpdateBackgroundImageSourceAsync(wrapperView.Content, imageSource, provider);
				return;
			}

			if (imageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var result = await service.GetImageAsync(imageSource);

				if (result != null)
				{
					var bg = new ImageVisual
					{
						URL = result.Value.ResourceUrl,
						FittingMode = FittingModeType.ScaleToFill
					};
					platformView.Background = bg.OutputVisualMap;
				}
			}
		}

		[Obsolete("IBorder is not used and will be removed in a future release.")]
		public static void UpdateBorder(this NView platformView, IView view)
		{
			if (view is IBorder border && platformView is WrapperView wrapperView)
				wrapperView.Border = border.Border;
		}

		public static void UpdateOpacity(this NView platformView, IView view) => platformView.UpdateOpacity(view.Opacity);

		internal static void UpdateOpacity(this NView platformView, double opacity) => platformView.Opacity = (float)opacity;

		public static void UpdateClip(this NView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this NView platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Shadow = view.Shadow;
		}

		public static void UpdateAutomationId(this NView platformView, IView view)
		{
			{
				//TODO: EvasObject.AutomationId is supported from tizen60.
				//platformView.AutomationId = view.AutomationId;
			}
		}

		public static void UpdateSemantics(this NView platformView, IView view)
		{
		}

		public static void InvalidateMeasure(this NView platformView, IView view)
		{
			if (platformView is LayoutViewGroup layoutViewGroup)
			{
				layoutViewGroup.SetNeedMeasureUpdate();
			}
			else if (platformView is ViewGroup viewGroup)
			{
				viewGroup.MarkChanged();
			}
			else if (view.ToPlatform().GetParent() is ViewGroup parentViewGroup)
			{
				parentViewGroup.MarkChanged();
			}
			else
			{
				platformView.Layout?.RequestLayout();
			}
		}

		public static void UpdateWidth(this NView platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateHeight(this NView platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateMinimumWidth(this NView platformView, IView view)
		{
			platformView.MinimumSize = new Tizen.NUI.Size2D(view.MinimumWidth.ToScaledPixel(), platformView.MinimumSize.Height);
		}

		public static void UpdateMinimumHeight(this NView platformView, IView view)
		{
			platformView.MinimumSize = new Tizen.NUI.Size2D(platformView.MinimumSize.Width, view.MinimumHeight.ToScaledPixel());
		}

		public static void UpdateMaximumWidth(this NView platformView, IView view)
		{
			// empty on purpose
			// NUI MaximumSize is not working properly
		}

		public static void UpdateMaximumHeight(this NView platformView, IView view)
		{
			// empty on purpose
			// NUI MaximumSize is not working properly
		}

		public static void UpdateInputTransparent(this NView platformView, IViewHandler handler, IView view)
		{
			platformView.Sensitive = !view.InputTransparent;
		}

		public static void UpdateSize(NView platformView, IView view)
		{
			if (!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
			{
				// Ignore the initial setting of the value; the initial layout will take care of it
				return;
			}
			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default (-1) width/height will be taken care of by GetDesiredSize
			platformView.UpdateSize(new Tizen.UIExtensions.Common.Size(view.Width, view.Height));
		}

		public static void UpdateToolTip(this NView platformView, ToolTip? tooltip)
		{
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

		internal static Rect GetPlatformViewBounds(this NView platformView)
		{
			if (platformView == null)
				return new Rect();
			var screenPosition = platformView.ScreenPosition;
			return new TRect(screenPosition.X, screenPosition.Y, platformView.SizeWidth, platformView.SizeHeight).ToDP();
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return platformView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this NView platformView)
			=> new Matrix4x4();

		internal static Graphics.Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rect GetBoundingBox(this NView? platformView)
		{
			if (platformView == null)
				return new Rect();

			return platformView.GetPlatformViewBounds();
		}

		internal static NView? GetParent(this NView? view)
		{
			return view?.GetParent() as NView;
		}

		internal static bool IsLoaded(this NView view) => view.IsOnWindow;

		internal static IDisposable OnLoaded(this NView view, Action action)
		{
			if (view.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler? routedEventHandler = null;
			ActionDisposable disposable = new ActionDisposable(() =>
			{
				if (routedEventHandler != null)
					view.AddedToWindow -= routedEventHandler;
			});

			routedEventHandler = (_, __) =>
			{
				disposable.Dispose();
				action();
			};

			view.AddedToWindow += routedEventHandler;
			return disposable;
		}

		internal static IDisposable OnUnloaded(this NView view, Action action)
		{
			if (!view.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler? routedEventHandler = null;
			ActionDisposable disposable = new ActionDisposable(() =>
			{
				if (routedEventHandler != null)
					view.RemovedFromWindow -= routedEventHandler;
			});

			routedEventHandler = (_, __) =>
			{
				disposable.Dispose();
				action();
			};

			view.RemovedFromWindow += routedEventHandler;
			return disposable;
		}

		internal static bool NeedsContainer(this IView? view)
		{
			if (view is IBorderView border)
				return border?.Shape != null || border?.Stroke != null;

			return false;
		}

		internal static T? GetChildAt<T>(this NView view, int index) where T : NView
		{
			return (T?)view.Children[index];
		}

		internal static bool HideSoftInput(this NView view) => SetKeyInputFocus(view, false);

		internal static bool ShowSoftInput(this NView view) => SetKeyInputFocus(view, true);

		internal static bool IsSoftInputShowing(this NView view)
		{
			return view.KeyInputFocus;
		}

		internal static bool SetKeyInputFocus(NView view, bool isShow)
		{
			view.KeyInputFocus = isShow;

			return view.KeyInputFocus;
		}

		internal static IWindow? GetHostedWindow(this IView? view)
			=> GetHostedWindow(view?.Handler?.PlatformView as View);

		internal static IWindow? GetHostedWindow(this UIView? view)
			=> GetHostedWindow(view?.Window);
	}
}
