using System;
using System.Linq;
using AppKit;
using System.Maui.Platform.MacOS;

namespace System.Maui.Platform.macOS.Extensions
{
	internal static class NSMenuExtensions
	{
		public static NSMenu ToNSMenu(this Menu menus, NSMenu nsMenu = null, Func<MenuItem, NSMenuItem> menuItemCreator = null)
		{
			if (nsMenu == null)
				nsMenu = new NSMenu(menus.Text ?? "");
			nsMenu.AutoEnablesItems = false;

			foreach (var menu in menus)
			{
				NSMenuItem menuItem = null;
				NSMenu subMenu = null;
				if (string.IsNullOrEmpty(menu.Text)) // handle menu with empty Text as the application menu
				{
					menuItem = nsMenu.Items.FirstOrDefault();
					subMenu = menuItem?.Submenu;
				}
				if (menuItem == null)
				{
					menuItem = new NSMenuItem(menu.Text ?? "");
					menuItem.Submenu = subMenu = new NSMenu(menu.Text ?? "");
				}

				foreach (var item in menu.Items)
				{
					var subMenuItem = item.ToNSMenuItem(menuItemCreator: menuItemCreator);
					GetAccelerators(subMenuItem, item);
					subMenu.AddItem(subMenuItem);
					item.PropertyChanged += (sender, e) => (sender as MenuItem)?.UpdateNSMenuItem(subMenuItem, new string[] { e.PropertyName });
				}
				if (!nsMenu.Items.Contains(menuItem))
					nsMenu.AddItem(menuItem);
				menu.ToNSMenu(subMenu, menuItemCreator);
			}
			return nsMenu;
		}


		public static NSMenuItem ToNSMenuItem(this MenuItem menuItem, int i = -1, Func<MenuItem, NSMenuItem> menuItemCreator = null)
		{
			NSMenuItem nsMenuItem = null;
			if (menuItemCreator == null)
				nsMenuItem = new NSMenuItem(menuItem.Text ?? "");
			else
				nsMenuItem = menuItemCreator(menuItem);
			if (i != -1)
				nsMenuItem.Tag = i;

			nsMenuItem.Enabled = menuItem.IsEnabled;
			nsMenuItem.Activated += (sender, e) => ((IMenuItemController)menuItem).Activate();
			_ = menuItem.ApplyNativeImageAsync(MenuItem.IconImageSourceProperty, image =>
			{
				if (image != null)
					nsMenuItem.Image = image;
			});

			return nsMenuItem;
		}



		public static void UpdateNSMenuItem(this MenuItem item, NSMenuItem menuItem, string[] properties)
		{
			foreach (var property in properties)
			{
				if (property.Equals(nameof(MenuItem.Text)))
				{
					menuItem.Title = item.Text;
				}
				if (property.Equals(nameof(MenuItem.IsEnabled)))
				{
					menuItem.Enabled = item.IsEnabled;
				}
				if (property.Equals(nameof(MenuItem.IconImageSource)))
				{
					_ = item.ApplyNativeImageAsync(MenuItem.IconImageSourceProperty, image =>
					{
						menuItem.Image = image;
					});
				}
			}
		}

		static void GetAccelerators(NSMenuItem nsMenuItem, MenuItem item)
		{
			var accelerator = MenuItem.GetAccelerator(item);

			if (accelerator == null)
				return;

			bool hasModifierMask = accelerator.Modifiers?.Count() > 1;

			if (hasModifierMask)
			{
				nsMenuItem.KeyEquivalentModifierMask = 0;

				for (int i = 0; i < accelerator.Modifiers.Count(); i++)
				{
					var modifierMask = accelerator.Modifiers.ElementAt(i).ToLower();
					switch (modifierMask)
					{
						case "ctrl":
							nsMenuItem.KeyEquivalentModifierMask = nsMenuItem.KeyEquivalentModifierMask | NSEventModifierMask.ControlKeyMask;
							break;
						case "cmd":
							nsMenuItem.KeyEquivalentModifierMask = nsMenuItem.KeyEquivalentModifierMask | NSEventModifierMask.CommandKeyMask;
							break;
						case "alt":
							nsMenuItem.KeyEquivalentModifierMask = nsMenuItem.KeyEquivalentModifierMask | NSEventModifierMask.AlternateKeyMask;
							break;
						case "shift":
							nsMenuItem.KeyEquivalentModifierMask = nsMenuItem.KeyEquivalentModifierMask | NSEventModifierMask.ShiftKeyMask;
							break;
						case "fn":
							nsMenuItem.KeyEquivalentModifierMask = nsMenuItem.KeyEquivalentModifierMask | NSEventModifierMask.FunctionKeyMask;
							break;
					}
				}
			}
			nsMenuItem.KeyEquivalent = accelerator.Keys.FirstOrDefault();
		}
	}
}
