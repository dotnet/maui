using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class KeyboardAcceleratorExtensions
	{
		internal const string MenuItemSelectedSelector = "MenuItemSelected:";

		internal static UIMenuElement CreateMenuItem(this IMenuFlyoutItem virtualView, IMauiContext mauiContext)
		{
			var index = MenuFlyoutItemHandler.menus.Count;
			var uiImage = virtualView.Source.GetPlatformMenuImage(mauiContext);
			var selector = new Selector(MenuItemSelectedSelector);

			var accelerator = virtualView.KeyboardAccelerators?[0];
			if (accelerator is null)
				return virtualView.CreateMenuItemCommand(index, uiImage, selector);

			var key = accelerator.Key;
			var modifiers = accelerator.Modifiers.ToUIKeyModifierFlags();

			// Single key accelerators (no modifier, i.e. "A") and multi-key accelerators are supported (>=1 modifier and 1 key only, i.e. Ctrl+F, Ctrl+Shift+F)
			return virtualView.CreateMenuItemKeyCommand(index, uiImage, selector, modifiers, key);
		}

		internal static UIKeyModifierFlags ToUIKeyModifierFlags(this KeyboardAcceleratorModifiers modifiers)
		{
			UIKeyModifierFlags modifierFlags = 0;

			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Shift))
				modifierFlags |= UIKeyModifierFlags.Shift;
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Ctrl))
				modifierFlags |= UIKeyModifierFlags.Control;
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Alt))
				modifierFlags |= UIKeyModifierFlags.Alternate;
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Cmd))
				modifierFlags |= UIKeyModifierFlags.Command;
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.AlphaShift))
				modifierFlags |= UIKeyModifierFlags.AlphaShift;
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.NumericPad))
				modifierFlags |= UIKeyModifierFlags.NumericPad;

			return modifierFlags;
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