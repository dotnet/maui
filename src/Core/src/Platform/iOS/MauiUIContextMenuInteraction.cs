using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

		// Long-press recognizers created by MAUI (gesture recognizers, CollectionView reordering). UIKit adds
		// its own long-press recognizers to some views, which don't do anything an app relies on.
		static readonly ConditionalWeakTable<UIGestureRecognizer, object> s_longPressRecognizers = new();

		/// <summary>
		/// Marks a long-press recognizer created by MAUI, so that a context menu doesn't take its long press over.
		/// </summary>
		internal static void MarkLongPressRecognizer(UIGestureRecognizer recognizer) =>
			s_longPressRecognizers.AddOrUpdate(recognizer, s_longPressRecognizers);

		/// <summary>
		/// Whether a long press at the location already does something else, which then keeps priority over
		/// the context menu: a long-press gesture or reordering on the view, an ancestor or the view under the
		/// finger, or text selection in a text input.
		/// </summary>
		internal bool DefersLongPress(CGPoint location)
		{
			if (View is not UIView view)
				return false;

			for (var current = view.HitTest(location, null) ?? view; current is not null; current = current.Superview)
			{
				if (current is UITextField { Enabled: true } || current is UITextView { Editable: true } || current is UITextView { Selectable: true })
					return true;

				if (current.GestureRecognizers is { } recognizers)
				{
					foreach (var recognizer in recognizers)
					{
						if (recognizer.Enabled && s_longPressRecognizers.TryGetValue(recognizer, out _))
							return true;
					}
				}

				if (current is UIWindow)
					break;
			}

			return false;
		}

		// Mac Catalyst opens context menus with a secondary click, which doesn't compete with long presses
		internal UIContextMenuConfiguration? GetConfigurationForMenu(CGPoint location) =>
			!OperatingSystem.IsMacCatalyst() && DefersLongPress(location) ? null : GetConfigurationForMenu();

		internal UIMenu? CreateMenu()
		{
			var handler = Handler;
			var virtualView = handler?.VirtualView;
			var contextFlyout = (virtualView as IContextFlyoutElement)?.ContextFlyout;
			var mauiContext = handler?.MauiContext;

			if (contextFlyout == null || mauiContext == null)
				return null;

			// Disabled views don't receive input, so they shouldn't open a context menu either.
			// Mac Catalyst keeps its existing behavior, where disabled controls can still open their menu.
			if (!OperatingSystem.IsMacCatalyst() && virtualView is IView view && (!view.IsEnabled || view.InputTransparent))
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
					return contextMenu.GetConfigurationForMenu(location);

				return null;
			}
		}
	}
}
