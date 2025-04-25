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
			var contextFlyout = (Handler?.VirtualView as IContextFlyoutElement)?.ContextFlyout;
			var mauiContext = Handler?.MauiContext;

			if (contextFlyout == null || mauiContext == null)
				return null;

			// Explicitly disconnect the existing handler to ensure the native context menu (UIMenu)
			// is recreated properly when the underlying menu items change dynamically.
			// Without this, the native context menu remains cached and won't reflect updates,
			// causing issues on iOS and Mac Catalyst platforms.
			contextFlyout.Handler?.DisconnectHandler();
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
