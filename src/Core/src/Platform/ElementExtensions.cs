using System;
using System.Collections.Generic;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
using BasePlatformType = ObjCRuntime.INativeObject;
using PlatformWindow = UIKit.UIWindow;
using PlatformApplication = UIKit.IUIApplicationDelegate;
#elif MONOANDROID
using PlatformView = Android.Views.View;
using BasePlatformType = Android.Content.Context;
using PlatformWindow = Android.App.Activity;
using PlatformApplication = Android.App.Application;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using BasePlatformType = WinRT.IWinRTObject;
using PlatformWindow = Microsoft.UI.Xaml.Window;
using PlatformApplication = Microsoft.UI.Xaml.Application;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using BasePlatformType = System.Object;
using PlatformWindow = Tizen.NUI.Window;
using PlatformApplication = Tizen.Applications.CoreApplication;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
using BasePlatformType = System.Object;
using IPlatformViewHandler = Microsoft.Maui.IViewHandler;
using PlatformWindow = System.Object;
using PlatformApplication = System.Object;
#endif

namespace Microsoft.Maui.Platform
{
	public static partial class ElementExtensions
	{
		static HashSet<Type> handlersWithConstructors = new HashSet<Type>();

		static IElementHandler? CreateTypeWithInjection(this Type viewType, IMauiContext mauiContext)
		{
			var handlerType = mauiContext.Handlers.GetHandlerType(viewType);

			if (handlerType == null)
				return null;

#if ANDROID
			if(mauiContext.Context != null)
			{
				return (IElementHandler)Extensions.DependencyInjection.
					ActivatorUtilities.CreateInstance(mauiContext.Services, handlerType, mauiContext.Context);
			}
#endif

			return (IElementHandler)Extensions.DependencyInjection.
				ActivatorUtilities.CreateInstance(mauiContext.Services, handlerType);
		}

		public static IElementHandler ToHandler(this IElement view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			//This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;

			if (handler?.MauiContext != null && handler.MauiContext != context)
				handler = null;


			// TODO Clean up this handler create. Handlers should probably create through the 
			// DI.Ext Service provider. We just register them all as transient? possibly?
			if (handler == null)
			{
				var viewType = view.GetType();
				try
				{
					if (handlersWithConstructors.Contains(viewType))
						handler = viewType.CreateTypeWithInjection(context);
					else
						handler = context.Handlers.GetHandler(viewType);
				}
				catch (MissingMethodException)
				{
					handler = viewType.CreateTypeWithInjection(context);
					if (handler != null)
						handlersWithConstructors.Add(view.GetType());
				}
			}

			if (handler == null)
				throw new HandlerNotFoundException(view);

			handler.SetMauiContext(context);

			view.Handler = handler;

			if (handler.VirtualView != view)
				handler.SetVirtualView(view);

			return handler;
		}

		internal static PlatformView ToPlatform(this IElement view)
		{
			if (view is IReplaceableView replaceableView && replaceableView.ReplacedView != view)
				return replaceableView.ReplacedView.ToPlatform();


			_ = view.Handler ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set on parent.");

			if (view.Handler is IViewHandler viewHandler)
			{
				if (viewHandler.ContainerView is PlatformView containerView)
					return containerView;

				if (viewHandler.PlatformView is PlatformView platformView)
					return platformView;
			}

			return (view.Handler?.PlatformView as PlatformView) ?? throw new InvalidOperationException($"Unable to convert {view} to {typeof(PlatformView)}");

		}

		public static PlatformView ToPlatform(this IElement view, IMauiContext context)
		{
			var handler = view.ToHandler(context);

			if (handler.PlatformView is not PlatformView result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(PlatformView)}");
			}

			return view.ToPlatform() ?? throw new InvalidOperationException($"Unable to convert {view} to {typeof(PlatformView)}");

		}

		static void SetHandler(this BasePlatformType nativeElement, IElement element, IMauiContext context)
		{
			_ = nativeElement ?? throw new ArgumentNullException(nameof(nativeElement));
			_ = element ?? throw new ArgumentNullException(nameof(element));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = element.Handler;
			if (handler?.MauiContext != null && handler.MauiContext != context)
				handler = null;

			if (handler == null)
				handler = context.Handlers.GetHandler(element.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for window {element}.");

			handler.SetMauiContext(context);

			element.Handler = handler;

			if (handler.VirtualView != element)
				handler.SetVirtualView(element);
		}

		public static void SetApplicationHandler(this PlatformApplication platformApplication, IApplication application, IMauiContext context) =>
			SetHandler(platformApplication, application, context);

		public static void SetWindowHandler(this PlatformWindow platformWindow, IWindow window, IMauiContext context) =>
			SetHandler(platformWindow, window, context);

#if WINDOWS || IOS || ANDROID || TIZEN
		internal static IWindow GetWindow(this IElement element) =>
		    (element as IViewWithWindow)?.Window ??
			(element as IView)?.GetHostedWindow() ??
			element.Handler?.MauiContext?.GetPlatformWindow()?.GetWindow() ??
			throw new InvalidOperationException("IWindow not found");
#endif

		internal static T? FindParentOfType<T>(this IElement element, bool includeThis = false)
	where T : IElement
		{
			if (includeThis && element is T view)
				return view;

			foreach (var parent in element.GetParentsPath())
			{
				if (parent is T parentView)
					return parentView;
			}

			return default;
		}

		static IEnumerable<IElement?> GetParentsPath(this IElement self)
		{
			IElement? current = self;

			while (current != null && current is not IApplication)
			{
				current = current.Parent;
				yield return current;
			}
		}
	}
}
