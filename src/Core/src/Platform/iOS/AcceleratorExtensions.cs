using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class AcceleratorExtensions
	{
		internal const string MenuItemSelectedSelector = "MenuItemSelected:";

		internal static UIMenuElement CreateMenuItem(this IMenuFlyoutItem virtualView, IMauiContext mauiContext)
		{
			var index = MenuFlyoutItemHandler.menus.Count;
			var uiImage = virtualView.Source.GetPlatformMenuImage(mauiContext);
			var selector = new Selector(MenuItemSelectedSelector);

			var accelerator = virtualView.Accelerators?[0];
			if (accelerator is null)
				return virtualView.CreateMenuItemCommand(index, uiImage, selector);

			var key = accelerator.Key;
			var modifiers = accelerator.Modifiers;

			// Single key accelerators (no modifier, i.e. "A") and multi-key accelerators are supported (>=1 modifier and 1 key only, i.e. Ctrl+F, Ctrl+Shift+F)
			UIKeyModifierFlags modifierFlags = 0;
			if (modifiers is not null && modifiers.Count > 0)
			{
				foreach (var modifier in modifiers)
				{
					var modifierMask = modifier.ToLowerInvariant();

					switch (modifierMask)
					{
						case "ctrl":
							modifierFlags |= UIKeyModifierFlags.Control;
							break;
						case "cmd":
							modifierFlags |= UIKeyModifierFlags.Command;
							break;
						case "alt":
							modifierFlags |= UIKeyModifierFlags.Alternate;
							break;
						case "shift":
							modifierFlags |= UIKeyModifierFlags.Shift;
							break;
					}
				}
			}

			return virtualView.CreateMenuItemKeyCommand(index, uiImage, selector, modifierFlags, key);
		}

		static UIMenuElement CreateMenuItemKeyCommand(this IMenuFlyoutItem virtualView, int index, UIImage? uiImage, Selector selector, UIKeyModifierFlags modifierFlags, string key)
		{
			var keyCommand = UIKeyCommand.Create(
				title: virtualView.Text,
				uiImage,
				selector,
				key,
				modifierFlags,
				new NSString(index.ToString()));

			keyCommand.WantsPriorityOverSystemBehavior = true;

			MenuFlyoutItemHandler.menus[index] = virtualView;

			return keyCommand;
		}

		static UIMenuElement CreateMenuItemCommand(this IMenuFlyoutItem virtualView, int index, UIImage? uiImage, Selector selector)
		{
			var command = UICommand.Create(
				title: virtualView.Text,
				uiImage,
				selector,
				new NSString(index.ToString()));

			MenuFlyoutItemHandler.menus[index] = virtualView;

			return command;
		}
	}
}