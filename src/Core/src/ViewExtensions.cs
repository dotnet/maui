using Microsoft.Maui.Graphics;
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
using System;
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

		public static Task<IScreenshotResult?> CaptureAsync(this IView view)
		{
#if PLATFORM
			if (view?.ToPlatform() is not PlatformView platformView)
				return Task.FromResult<IScreenshotResult?>(null);

			if (!Screenshot.Default.IsCaptureSupported)
				return Task.FromResult<IScreenshotResult?>(null);

			return CaptureAsync(platformView);
#else
			return Task.FromResult<IScreenshotResult?>(null);
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
