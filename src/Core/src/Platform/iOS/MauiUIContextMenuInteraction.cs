using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	[SupportedOSPlatform("ios13.0")]
	[SupportedOSPlatform("maccatalyst13.0.0")]
	[UnsupportedOSPlatform("tvos")]
	class MauiUIContextMenuInteraction : UIContextMenuInteraction
	{
		WeakReference<IElementHandler> _handler;

		// Store a reference to the platform delegate so that it is not garbage collected
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Delegate is weak on Objective-C side")]
		readonly IUIContextMenuInteractionDelegate? _uiContextMenuInteractionDelegate;

		public MauiUIContextMenuInteraction(IElementHandler handler)
			: base(CreateDelegate(out var del))
		{
			_uiContextMenuInteractionDelegate = del;
			_handler = new WeakReference<IElementHandler>(handler);
		}

		static IUIContextMenuInteractionDelegate CreateDelegate(out IUIContextMenuInteractionDelegate del) =>
			del = new FlyoutUIContextMenuInteractionDelegate();

		public UIContextMenuConfiguration? GetConfigurationForMenu()
		{
			// A flyout with only separators has nothing to show
			if (CreateMenu() is not UIMenu { Children.Length: > 0 } uiMenu)
				return null;

			return UIContextMenuConfiguration.Create(
						identifier: null,
						previewProvider: null,
						actionProvider: _ => uiMenu);
		}

		internal UIMenu? CreateMenu()
		{
			var handler = Handler;
			var virtualView = handler?.VirtualView;
			var contextFlyout = (virtualView as IContextFlyoutElement)?.ContextFlyout;
			var mauiContext = handler?.MauiContext;

			if (contextFlyout == null || mauiContext == null)
				return null;

			// Disabled views don't receive input, so they shouldn't open a context menu either
			if (virtualView is IView view && (!view.IsEnabled || view.InputTransparent))
				return null;

			if (contextFlyout is IMenuFlyout menuFlyout && menuFlyout.Count == 0)
				return null;

			// The menu is rebuilt every time it opens, so it always reflects the current state of the items
			contextFlyout.Handler?.DisconnectHandler();
			var contextFlyoutHandler = contextFlyout.ToHandler(mauiContext);
			var contextFlyoutPlatformView = contextFlyoutHandler.PlatformView;

			return contextFlyoutPlatformView as UIMenu;
		}

		IElementHandler? Handler
		{
			get
			{
				if (_handler.TryGetTarget(out var handler))
				{
					return handler;
				}

				return null;
			}
		}

		sealed class FlyoutUIContextMenuInteractionDelegate : NSObject, IUIContextMenuInteractionDelegate
		{
			public FlyoutUIContextMenuInteractionDelegate()
			{
			}

			public UIContextMenuConfiguration? GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
			{
				if (interaction is MauiUIContextMenuInteraction contextMenu)
					return contextMenu.GetConfigurationForMenu();

				return null;
			}
		}
	}
}
