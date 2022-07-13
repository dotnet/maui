using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		public static void MapContextFlyout(IViewHandler handler, IView view)
		{
			if (view is IContextFlyoutContainer contextFlyoutContainer)
			{
				MapContextFlyout(handler, contextFlyoutContainer);
			}
		}
#pragma warning disable CA1416 // Validate platform compatibility

		// Store a reference to the platform delegate so that it is not garbage collected
		private IUIContextMenuInteractionDelegate? _uiContextMenuInteractionDelegate;

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutContainer contextFlyoutContainer)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

			if (contextFlyoutContainer.ContextFlyout != null && contextFlyoutContainer.ContextFlyout.Any())
			{
				// REVIEW: I'd like to call this code, but it throws because the ContextFlyout doesn't yet have a
				// handler associated, so calling 'ToPlatform()' throws an exception. Instead, I've copied most of
				// the code of those code paths and changed it to make it work here to create the ContextFlyoutHandler.
				//var platformViewAttempt = contextFlyoutContainer.ContextFlyout.ToPlatform() ?? throw new InvalidOperationException($"Unable to convert view to {typeof(PlatformView)}");

				// This will set the MauiContext and get everything created first
				var handler2 = contextFlyoutContainer.ContextFlyout.ToHandler(handler.MauiContext);

				object? contextFlyoutPlatformView;
				if (contextFlyoutContainer.ContextFlyout is IReplaceableView replaceableView && replaceableView.ReplacedView != contextFlyoutContainer.ContextFlyout)
					contextFlyoutPlatformView = replaceableView.ReplacedView.ToPlatform();

				_ = contextFlyoutContainer.ContextFlyout.Handler ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set on parent.");

				if (contextFlyoutContainer.ContextFlyout.Handler is IViewHandler viewHandler)
				{
					if (viewHandler.ContainerView is PlatformView containerView)
						contextFlyoutPlatformView = containerView;

					if (viewHandler.PlatformView is PlatformView platformView)
						contextFlyoutPlatformView = platformView;
				}

				contextFlyoutPlatformView = contextFlyoutContainer.ContextFlyout.Handler?.PlatformView;

				if (handler.PlatformView is PlatformView uiView && contextFlyoutPlatformView is UIMenu uiMenu)
				{
					var viewHandlerObj = handler as ViewHandler;
					viewHandlerObj!._uiContextMenuInteractionDelegate = new FlyoutUIContextMenuInteractionDelegate(
							() =>
							{
								return UIContextMenuConfiguration.Create(
									identifier: null,
									previewProvider: null,
									actionProvider: _ => uiMenu);
							});
					var newFlyout = new UIContextMenuInteraction(
						@delegate: viewHandlerObj!._uiContextMenuInteractionDelegate);

					uiView.AddInteraction(newFlyout);
				}
			}
		}

		private sealed class FlyoutUIContextMenuInteractionDelegate : NSObject, IUIContextMenuInteractionDelegate
		{
			private readonly Func<UIContextMenuConfiguration> _menuConfigurationFunc;

			public FlyoutUIContextMenuInteractionDelegate(Func<UIContextMenuConfiguration> menuConfigurationFunc)
			{
				_menuConfigurationFunc = menuConfigurationFunc;
			}

			public UIContextMenuConfiguration? GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
			{
				return _menuConfigurationFunc();
			}
		}
#pragma warning restore CA1416 // Validate platform compatibility

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
			handler.ToPlatform().UpdateBackgroundLayerFrame();
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		internal static void UpdateTransformation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public virtual bool NeedsContainer
		{
			get
			{
				return VirtualView?.Clip != null || VirtualView?.Shadow != null || (VirtualView as IBorder)?.Border != null;
			}
		}
	}
}