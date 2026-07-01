using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using System.Collections.Generic;
using System.IO;
#if (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
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

namespace Microsoft.Maui
{
	public static partial class ViewExtensions
	{
		public static void DisconnectHandlers(this IView view)
		{
			// For our first go here
			// My thinking is to build a flat list of all views in the tree
			// And then iterate down the list disconnecting handlers.
			// This gives me a stable list of views to call disconnecthandler on
			// I'm assuming as this PR evolves we'll probably add some interfaces
			// that allow handlers to manage their own children and disconnect flow
			List<IView> _flatList = new List<IView>();
			BuildFlatList(view, _flatList);

			foreach (var viewToDisconnect in _flatList)
			{
				viewToDisconnect.Handler?.DisconnectHandler();
			}

			void BuildFlatList(IView view, List<IView> flatList)
			{
				if (view is IHandlerDisconnectPolicies HandlerPropertiess && HandlerPropertiess.DisconnectPolicy == HandlerDisconnectPolicy.Manual)
				{
					return;
				}

				flatList.Add(view);

				if (view is IVisualTreeElement vte)
				{
					foreach (var child in vte.GetVisualChildren())
					{
						if (child is IView childView)
						{
							BuildFlatList(childView, flatList);
						}
					}
				}
			}
		}

		/// <summary>
		/// Captures a screenshot of the specified <paramref name="view"/>.
		/// </summary>
		/// <remarks>
		/// On non-built-in platform TFMs (e.g. <c>net10.0-macos</c> AppKit backends,
		/// <c>net10.0</c> Linux/GTK backends) where MAUI does not ship a screenshot
		/// implementation, capture is routed through a keyed DI hook. Third-party
		/// platform backends can opt in by registering a
		/// <see cref="Func{T, TResult}"/> of <see cref="object"/> to
		/// <c>Task&lt;IScreenshotResult?&gt;</c> under the service key
		/// <c>"Microsoft.Maui.ViewCapture"</c>:
		/// <code>
		/// builder.Services.AddKeyedSingleton&lt;Func&lt;object, Task&lt;IScreenshotResult?&gt;&gt;&gt;(
		///     "Microsoft.Maui.ViewCapture",
		///     (_, _) =&gt; platformView =&gt; ((AppKit.NSView)platformView).CaptureAsync());
		/// </code>
		/// If no hook is registered (or the <see cref="IElementHandler.PlatformView"/>
		/// is <see langword="null"/>), the returned task resolves to
		/// <see langword="null"/>.
		/// </remarks>
		public static Task<IScreenshotResult?> CaptureAsync(this IView view)
		{
#if PLATFORM
			if (view?.ToPlatform() is not PlatformView platformView)
				return Task.FromResult<IScreenshotResult?>(null);

			if (!Screenshot.Default.IsCaptureSupported)
				return Task.FromResult<IScreenshotResult?>(null);

			return CaptureAsync(platformView);
#else
			return ScreenshotDispatch.CaptureAsync(view?.Handler, ScreenshotDispatch.ViewCaptureKey);
#endif
		}


#if PLATFORM
		async static Task<IScreenshotResult?> CaptureAsync(PlatformView window) =>
			await Screenshot.Default.CaptureAsync(window);
#endif

#if !TIZEN
		internal static bool NeedsContainer(this IView? view)
		{
			if (view?.Clip != null || view?.Shadow != null)
				return true;

#if ANDROID
			if (view?.InputTransparent == true)
				return true;
#endif

#if ANDROID || IOS
#pragma warning disable CS0618 // Type or member is obsolete
			if (view is IBorder border && border.Border != null)
				return true;
#pragma warning restore CS0618 // Type or member is obsolete
#elif WINDOWS
			if (view is IBorderView border)
				return border?.Shape != null || border?.Stroke != null;
#endif
			return false;
		}
#endif

	}
}
