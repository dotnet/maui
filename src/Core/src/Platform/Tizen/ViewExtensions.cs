using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using static Microsoft.Maui.Primitives.Dimension;
using Rect = Microsoft.Maui.Graphics.Rect;
using System.Linq;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void UpdateIsEnabled(this NView nativeView, IView view)
		{
			nativeView.SetEnable(view.IsEnabled);
		}

		public static void UpdateVisibility(this NView nativeView, IView view)
		{
			// TODO: Implement Focus on Tizen (ref #4588)
		}

		public static void Unfocus(this EvasObject platformView, IView view)
		{
			// TODO: Implement Unfocus on Tizen (ref #4588)
		}

		public static void UpdateVisibility(this EvasObject platformView, IView view)
		{
			if (view.Visibility.ToPlatformVisibility())
			{
				platformView.Show();
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

		public static void UpdateBackground(this NView nativeView, IView view)
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
			else if (platformView is BorderView borderView)
			{
				borderView.ContainerView?.UpdateBackground(paint);
			}
			else if (paint is not null)
			{
				platformView.UpdateBackgroundColor(paint.ToPlatform());
			}
		}

		public static void UpdateOpacity(this NView nativeView, IView view)
		{
			nativeView.Opacity = (float)view.Opacity;
		}

		public static void UpdateClip(this NView nativeView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this NView nativeView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Shadow = view.Shadow;
		}

		public static void UpdateAutomationId(this NView nativeView, IView view)
		{
			{
				//TODO: EvasObject.AutomationId is supported from tizen60.
				//platformView.AutomationId = view.AutomationId;
			}
		}

		public static void UpdateSemantics(this NView nativeView, IView view)
		{
		}

		public static void InvalidateMeasure(this NView nativeView, IView view)
		{
			nativeView.Layout?.RequestLayout();
		}

		public static void UpdateWidth(this NView nativeView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateHeight(this NView nativeView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateMinimumWidth(this NView nativeView, IView view)
		{
			if (view.MinimumWidth.ToScaledPixel() > nativeView.MinimumSize.Width)
				nativeView.MinimumSize = new Tizen.NUI.Size2D(view.MinimumWidth.ToScaledPixel(), nativeView.MinimumSize.Height);
		}

		public static void UpdateMinimumHeight(this NView nativeView, IView view)
		{
			if (view.MinimumHeight.ToScaledPixel() > nativeView.MinimumSize.Height)
				nativeView.MinimumSize = new Tizen.NUI.Size2D(nativeView.MinimumSize.Width, view.MinimumHeight.ToScaledPixel());
		}

		public static void UpdateMaximumWidth(this NView nativeView, IView view)
		{
			// empty on purpose
			// NUI MaximumSize is not working properly
		}

		public static void UpdateMaximumHeight(this NView nativeView, IView view)
		{
			// empty on purpose
			// NUI MaximumSize is not working properly
		}

		public static void UpdateInputTransparent(this EvasObject platformView, IViewHandler handler, IView view)
		{
			// TODO
			//platformView.PassEvents = view.InputTransparent;
		}

		public static void UpdateSize(EvasObject platformView, IView view)
		{
			if (!IsExplicitSet(view.Width) || !IsExplicitSet(view.Height))
			{
				// Ignore the initial setting of the value; the initial layout will take care of it
				return;
			}
			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default (-1) width/height will be taken care of by GetDesiredSize
			nativeView.UpdateSize(new Tizen.UIExtensions.Common.Size(view.Width, view.Height));
		}

		// TODO : Should consider a better way to determine that the view has been loaded/unloaded.
		internal static bool IsLoaded(this EvasObject view) =>
			view.IsRealized;

		internal static IDisposable OnLoaded(this EvasObject view, Action action)
		{
			if (view.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler? renderPostEventHandler = null;
			ActionDisposable disposable = new ActionDisposable(() =>
			{
				if (renderPostEventHandler != null)
					view.RenderPost -= renderPostEventHandler;
			});

			renderPostEventHandler = (_, __) =>
			{
				disposable.Dispose();
				action();
			};

			view.RenderPost += renderPostEventHandler;
			return disposable;
		}

		internal static IDisposable OnUnloaded(this EvasObject view, Action action)
		{
			if (!view.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler? deletedEventHandler = null;
			ActionDisposable disposable = new ActionDisposable(() =>
			{
				if (deletedEventHandler != null)
					view.Deleted -= deletedEventHandler;
			});

			deletedEventHandler = (_, __) =>
			{
				disposable.Dispose();
				action();
			};

			view.Deleted += deletedEventHandler;
			return disposable;
		}

		internal static Rectangle GetPlatformViewBounds(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
			{
				return new Rect();
			}

			return platformView.GetPlatformViewBounds();
		}

		internal static Rect GetPlatformViewBounds(this EvasObject platformView)
		{
			if (platformView == null)
				return new Rect();

			return platformView.Geometry.ToDP();
		}

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return platformView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this EvasObject platformView)
			=> new Matrix4x4();

		internal static Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Rect GetBoundingBox(this EvasObject? platformView)
		{
			if (platformView == null)
				return new Rect();

			return platformView.Geometry.ToDP();
		}

		internal static EvasObject? GetParent(this EvasObject? view)
		{
			return view?.Parent;
		}
	}
}
