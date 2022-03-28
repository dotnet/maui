using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using ElmSharp;
using ElmSharp.Accessible;
using Tizen.UIExtensions.ElmSharp;
using static Microsoft.Maui.Primitives.Dimension;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		public static void UpdateIsEnabled(this EvasObject platformView, IView view)
		{
			if (!(platformView is Widget widget))
				return;

			widget.IsEnabled = view.IsEnabled;
		}

		public static void Focus(this EvasObject platformView, FocusRequest request)
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

		public static void UpdateBackground(this EvasObject platformView, IView view)
		{
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

		public static void UpdateBorder(this EvasObject platformView, IView view)
		{
			var border = (view as IBorder)?.Border;

			if (platformView is WrapperView wrapperView)
				wrapperView.Border = border;
		}

		public static void UpdateOpacity(this EvasObject platformView, IView view)
		{
			if (platformView is Widget widget)
			{
				widget.Opacity = (int)(view.Opacity * 255.0);
			}
		}

		public static void UpdateClip(this EvasObject platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Clip = view.Clip;
		}

		public static void UpdateShadow(this EvasObject platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
				wrapper.Shadow = view.Shadow;
		}

		public static void UpdateAutomationId(this EvasObject platformView, IView view)
		{
			{
				//TODO: EvasObject.AutomationId is supported from tizen60.
				//platformView.AutomationId = view.AutomationId;
			}
		}

		public static void UpdateSemantics(this EvasObject platformView, IView view)
		{
			var semantics = view.Semantics;
			var accessibleObject = platformView as IAccessibleObject;

			if (semantics == null || accessibleObject == null)
				return;

			accessibleObject.Name = semantics.Description;
			accessibleObject.Description = semantics.Hint;
		}

		public static void InvalidateMeasure(this EvasObject platformView, IView view)
		{
			platformView.MarkChanged();
		}

		public static void UpdateWidth(this EvasObject platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateHeight(this EvasObject platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateMinimumWidth(this EvasObject platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateMinimumHeight(this EvasObject platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateMaximumWidth(this EvasObject platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateMaximumHeight(this EvasObject platformView, IView view)
		{
			UpdateSize(platformView, view);
		}

		public static void UpdateInputTransparent(this EvasObject platformView, IViewHandler handler, IView view)
		{
			platformView.PassEvents = view.InputTransparent;
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
			platformView.Resize(view.Width.ToScaledPixel(), view.Height.ToScaledPixel());
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
	}
}
