using System;
using System.Linq;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class AcceleratorExtensions
	{
		internal static UIMenuElement CreateMenuItem(this IMenuFlyoutItem virtualView, IMauiContext mauiContext)
		{
			int index = MenuFlyoutItemHandler.menus.Count;
			UIImage? uiImage = virtualView.Source.GetPlatformMenuImage(mauiContext);
			var selector = new Selector($"MenuItem{index}:");

			bool selectorFound =
				MauiUIApplicationDelegate.Current.RespondsToSelector(selector);

			if (!selectorFound)
			{
				throw new InvalidOperationException(
					$"By default we only support 50 MenuItems. You can add more by adding the following code to {MauiUIApplicationDelegate.Current.GetType()}\n\n" +
					$"[Export(\"MenuItem{index}: \")]\n" +
					$"internal void MenuItem{index}(UICommand uICommand)\n" +
					"{\n" +
					"	uICommand.SendClicked();\n" +
					"}");
			}

			var accelerator = virtualView.Accelerators?[0];
			if (accelerator is null)
				return virtualView.CreateMenuItemCommand(index, uiImage, selector);

			var key = accelerator.Key;
			var modifiers = accelerator.Modifiers;

			UIKeyModifierFlags modifierFlags = 0;

			// Single key accelerators (no modifier, i.e. "A") and multi-key accelerators are supported (>=1 modifier and 1 key only, i.e. Ctrl+F, Ctrl+Shift+F)
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