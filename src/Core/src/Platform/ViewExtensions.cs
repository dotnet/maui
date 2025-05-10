using System;
using System.Numerics;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using System.IO;
using System.Collections.Generic;

#if (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using IPlatformViewHandler = Microsoft.Maui.IViewHandler;
#endif
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using ParentView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
using ParentView = Android.Views.IViewParent;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using ParentView = Microsoft.UI.Xaml.DependencyObject;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using ParentView = Tizen.NUI.BaseComponents.View;
#else
using PlatformView = System.Object;
using ParentView = System.Object;
#endif

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Extension methods for <see cref="IView"/>s, providing animatable scaling, rotation, and layout functions.
	/// </summary>
	public static partial class ViewExtensions
	{
		internal static Vector3 ExtractPosition(this Matrix4x4 matrix) => matrix.Translation;

		internal static Vector3 ExtractScale(this Matrix4x4 matrix) => new Vector3(matrix.M11, matrix.M22, matrix.M33);

		internal static double ExtractAngleInRadians(this Matrix4x4 matrix) => Math.Atan2(matrix.M21, matrix.M11);

		internal static double ExtractAngleInDegrees(this Matrix4x4 matrix) => ExtractAngleInRadians(matrix) * 180 / Math.PI;


		/// <summary>
		/// Gets the platform-specific view handler for the specified view.
		/// </summary>
		/// <param name="view">The view to get the handler for.</param>
		/// <param name="context">The Maui context used to create the handler.</param>
		/// <returns>The platform-specific view handler.</returns>
		public static IPlatformViewHandler ToHandler(this IView view, IMauiContext context) =>
			(IPlatformViewHandler)ElementExtensions.ToHandler(view, context);

		internal static T? GetParentOfType<T>(this ParentView? view)
#if ANDROID
			where T : class, ParentView
#elif PLATFORM
			where T : ParentView
#else
			where T : class
#endif
		{
			if (view is T t)
				return t;

			while (view != null)
			{
				T? parent = view?.GetParent() as T;
				if (parent != null)
					return parent;

				view = view?.GetParent() as ParentView;
			}

			return default;
		}

		// Only Windows and Android have different types for the Parent type
#if WINDOWS || ANDROID
		internal static ParentView? FindParent(this PlatformView? view, Func<ParentView?, bool> searchExpression)
		{
			if (view?.Parent is ParentView pv)
			{
				if (searchExpression(pv))
					return pv;

				return pv.FindParent(searchExpression);
			}

			return default;
		}
#else
		internal
#endif
		static ParentView? FindParent(this ParentView? view, Func<ParentView?, bool> searchExpression)
		{
			while (view != null)
			{
				var parent = view?.GetParent() as ParentView;
				if (searchExpression(parent))
					return parent;

				view = view?.GetParent() as ParentView;
			}

			return default;
		}

#if WINDOWS || ANDROID
		internal static T? GetParentOfType<T>(this PlatformView view)
#if ANDROID
			where T : class, ParentView
#elif PLATFORM
			where T : ParentView
#else
			where T : class
#endif
		{
			if (view is T t)
				return t;

			return view.GetParent()?.GetParentOfType<T>();
		}
#endif

		internal static bool IsThisMyPlatformView(this IElement? element, PlatformView platformView)
		{
			if (element is not null &&
				element.Handler is IPlatformViewHandler pvh)
			{
				return pvh.PlatformView == platformView || pvh.ContainerView == platformView;
			}

			return false;
		}

		internal static IDisposable OnUnloaded(this IElement element, Action action)
		{
#if PLATFORM
			if (element.Handler is IPlatformViewHandler platformViewHandler &&
				platformViewHandler.PlatformView is not null)
			{
#if IOS
				if (platformViewHandler.ContainerView is IUIViewLifeCycleEvents)
					return platformViewHandler.ContainerView.OnUnloaded(action);
#endif
				return platformViewHandler.PlatformView.OnUnloaded(action);
			}

			throw new InvalidOperationException("Handler is not set on element");
#else
			throw new NotImplementedException();
#endif
		}

		internal static IDisposable OnLoaded(this IElement element, Action action)
		{
#if PLATFORM
			if (element.Handler is IPlatformViewHandler platformViewHandler &&
				platformViewHandler.PlatformView is not null)
			{
#if IOS
				if (platformViewHandler.ContainerView is IUIViewLifeCycleEvents)
					return platformViewHandler.ContainerView.OnLoaded(action);
#endif
				return platformViewHandler.PlatformView.OnLoaded(action);
			}

			throw new InvalidOperationException("Handler is not set on element");
#else
			throw new NotImplementedException();
#endif
		}

#if PLATFORM
		internal static Task OnUnloadedAsync(this PlatformView platformView, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			IDisposable? token = null;
			token = platformView.OnUnloaded(() =>
			{
				taskCompletionSource.SetResult(true);
				token?.Dispose();
				token = null;
			});

			return taskCompletionSource.Task.WaitAsync(timeOut.Value);
		}

		internal static Task OnLoadedAsync(this PlatformView platformView, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			IDisposable? token = null;
			token = platformView.OnLoaded(() =>
			{
				taskCompletionSource.SetResult(true);
				token?.Dispose();
				token = null;
			});
			return taskCompletionSource.Task.WaitAsync(timeOut.Value);
		}
#endif
		internal static bool IsLoadedOnPlatform(this IElement element)
		{
			if (element.Handler is not IPlatformViewHandler pvh)
				return false;

#if PLATFORM
			return pvh.PlatformView?.IsLoaded() == true;
#else
			return true;
#endif

		}


#if PLATFORM
		internal static T? FindDescendantView<T>(this PlatformView view, Func<PlatformView, bool> predicate) where T : PlatformView
		{
			var queue = new Queue<PlatformView>();
			queue.Enqueue(view);

			while (queue.Count > 0)
			{
				var descendantView = queue.Dequeue();

				if (descendantView is T result && predicate.Invoke(result))
					return result;

				int i = 0;
				PlatformView? child;
				while ((child = descendantView?.GetChildAt<PlatformView>(i)) is not null)
				{
#if TIZEN
					// I had to add this check for Tizen to compile.
					// I think Tizen isn't accounting for the null check
					// in the while loop correctly
					if (child is null)
						break;
#endif
					queue.Enqueue(child);
					i++;
				}
			}

			return null;
		}
#endif
	}
}
