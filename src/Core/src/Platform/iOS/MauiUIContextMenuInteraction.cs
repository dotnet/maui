using System;
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
		IUIContextMenuInteractionDelegate? _uiContextMenuInteractionDelegate;

		public MauiUIContextMenuInteraction(IElementHandler handler)
			: base(new FlyoutUIContextMenuInteractionDelegate())
		{
			_uiContextMenuInteractionDelegate = Delegate;
			_handler = new WeakReference<IElementHandler>(handler);
		}

		public UIContextMenuConfiguration? GetConfigurationForMenu()
		{
			var contextFlyout = (Handler?.VirtualView as IContextFlyoutElement)?.ContextFlyout;
			var mauiContext = Handler?.MauiContext;

			if (contextFlyout == null || mauiContext == null)
				return null;

			var contextFlyoutHandler = contextFlyout.ToHandler(mauiContext);
			var contextFlyoutPlatformView = contextFlyoutHandler.PlatformView;

			if (contextFlyoutPlatformView is UIMenu uiMenu)
			{
				return UIContextMenuConfiguration.Create(
							identifier: null,
							previewProvider: null,
							actionProvider: _ => uiMenu);
			}

			return null;
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
