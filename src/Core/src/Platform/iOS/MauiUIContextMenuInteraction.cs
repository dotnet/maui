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

		// Long-press recognizers that keep priority over context menus: MAUI's CollectionView reordering, which
		// isn't based on UIKit's drag and drop that context menus work together with
		static readonly ConditionalWeakTable<UIGestureRecognizer, object> s_longPressRecognizers = new();

		/// <summary>
		/// Marks a long-press recognizer that keeps priority over context menus of the views inside its view.
		/// </summary>
		internal static void MarkLongPressRecognizer(UIGestureRecognizer recognizer) =>
			s_longPressRecognizers.AddOrUpdate(recognizer, s_longPressRecognizers);

		/// <summary>
		/// Whether a long press at the location does something else instead of opening the context menu.
		/// </summary>
		/// <remarks>
		/// UIKit decides between the context menu and other long presses, like it does in native apps: the
		/// context menu wins over long-press gesture recognizers and text selection. Only CollectionView
		/// reordering keeps working on items that have a context menu, and in a collection inside a view with one.
		/// </remarks>
		internal bool DefersLongPress(CGPoint location)
		{
			var view = View;
			var pressed = view is not null && view.PointInside(location, null) ? view.HitTest(location, null) : null;

			for (var current = pressed ?? view; current is not null; current = current.Superview)
			{
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

		internal UIContextMenuConfiguration? GetConfigurationForMenu(CGPoint location)
		{
			// Text inputs keep selecting text; the items are added to their edit menu instead
			var view = View;
			if (view is not null && view.PointInside(location, null) && ContextFlyoutEditMenu.IsTextInput(view.HitTest(location, null), view))
				return null;

			// Mac Catalyst opens context menus with a secondary click, which doesn't compete with long presses
			return !OperatingSystem.IsMacCatalyst() && DefersLongPress(location) ? null : GetConfigurationForMenu();
		}

		/// <summary>
		/// The context menu interaction of the closest view, starting at the given one, whose context flyout can show.
		/// </summary>
		internal static MauiUIContextMenuInteraction? FindClosest(UIView view)
		{
			for (var current = view; current is not null; current = current.Superview)
			{
				foreach (var interaction in current.Interactions)
				{
					if (interaction is MauiUIContextMenuInteraction menuInteraction && menuInteraction.CanShow())
						return menuInteraction;
				}

				if (current is UIWindow)
					break;
			}

			return null;
		}

		bool CanShow()
		{
			var virtualView = Handler?.VirtualView;
			if ((virtualView as IContextFlyoutElement)?.ContextFlyout is not IMenuFlyout menuFlyout || Handler?.MauiContext is null)
				return false;

			if (!OperatingSystem.IsMacCatalyst() && virtualView is IView view && (!view.IsEnabled || view.InputTransparent))
				return false;

			for (int i = 0; i < menuFlyout.Count; i++)
			{
				if (menuFlyout[i] is not IMenuFlyoutSeparator)
					return true;
			}

			return false;
		}

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
