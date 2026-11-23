using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutSubItemHandler
	{
		string? _contextText;
		IImageSource? _contextSource;
		bool _contextMenu;
		bool _refreshingContextMenu;

		protected override UIMenu CreatePlatformElement()
		{
			IElement? parent = VirtualView.Parent;
			while (parent is not null && parent is not IMenuFlyout)
				parent = parent.Parent;

			_contextMenu = parent is IMenuFlyout;
			if (_contextMenu)
			{
				_contextText = VirtualView.Text;
				_contextSource = VirtualView.Source;
				using var contents = VirtualView.ToPlatformMenu(MauiContext!);
				return UIMenu.Create(VirtualView.Text,
					VirtualView.Source.GetPlatformMenuImage(MauiContext!),
					UIMenuIdentifier.None, UIMenuOptions.SingleSelection, contents.Children);
			}

			var menuBar = VirtualView.FindParentOfType<IMenuBar>();

			IUIMenuBuilder? uIMenuBuilder = null;
			if (menuBar?.Handler?.PlatformView is IUIMenuBuilder builder)
			{
				uIMenuBuilder = builder;
			}

			var menu =
				VirtualView.ToPlatformMenu(
					VirtualView.Text,
					VirtualView.Source,
					MauiContext!,
					uIMenuBuilder);

			return menu;
		}

		internal static void MapContextMetadata(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			if (handler is MenuFlyoutSubItemHandler menuHandler && menuHandler._contextMenu &&
				(!string.Equals(menuHandler._contextText, view.Text, StringComparison.Ordinal) ||
				 menuHandler._contextSource != view.Source))
			{
				// Reconnect the same handler to replace the immutable UIMenu and replay its mappings.
				var previous = menuHandler.PlatformView;
				menuHandler._refreshingContextMenu = true;
				try
				{
					((IElementHandler)menuHandler).DisconnectHandler();
					menuHandler.SetVirtualView(view);
				}
				finally
				{
					menuHandler._refreshingContextMenu = false;
				}
				previous.Dispose();
			}
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler?.PlatformView?.UpdateIsEnabled(view);
		}

		public void Add(IMenuElement view)
		{
			Rebuild();
		}

		public void Remove(IMenuElement view)
		{
			Rebuild();
		}

		public void Clear()
		{
			Rebuild();
		}

		public void Insert(int index, IMenuElement view)
		{
			Rebuild();
		}

		static void Rebuild()
		{
			// For context flyout support this likely also needs some logic like in MenuFlyoutItemHandler.iOS.cs where
			// it follows one code path for main menus (this existing code), and a different code path for context menus that
			// rebuilds the UIMenu of the context menu.
			// https://github.com/dotnet/maui/issues/9359
			MenuBarHandler.Rebuild();
		}
	}
}
