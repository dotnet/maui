using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutSubItemHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
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

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			UpdateIsEnabled(handler, view);
		}

		static void UpdateIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			var menu = handler.PlatformView;

			if (menu is null)
				return;

			foreach (var child in menu.Children)
			{
				using (var menuEnumerator = view.GetEnumerator())
				{
					while (menuEnumerator.MoveNext())
					{
						var menuElement = menuEnumerator.Current;

						if (child.Title == menuElement.Text)
						{
							if (child is UIAction action)
								action.Attributes = menuElement.ToUIMenuElementAttributes();

							if (child is UICommand command)
								command.Attributes = menuElement.ToUIMenuElementAttributes();
						}
					}
				}
			}
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

		void Rebuild()
		{
			// For context flyout support this likely also needs some logic like in MenuFlyoutItemHandler.iOS.cs where
			// it follows one code path for main menus (this existing code), and a different code path for context menus that
			// rebuilds the UIMenu of the context menu.
			// https://github.com/dotnet/maui/issues/9359
			MenuBarHandler.Rebuild();
		}
	}
}
