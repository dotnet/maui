using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Security.Cryptography.Certificates;
using Windows.System;

namespace Microsoft.Maui.Platform
{
	public static class KeyboardAcceleratorExtensions
	{
		public static void UpdateAccelerator(this MenuFlyoutItemBase platformView, IMenuFlyoutItem menuFlyoutItem)
		{
#if NETCOREAPP2_1_OR_GREATER
			var keyboardAccelerators = menuFlyoutItem.Accelerator?.ToPlatform();
#else
			var keyboardAccelerators = menuFlyoutItem.Accelerator.ToPlatform();
#endif

			if (keyboardAccelerators is null)
				return;

			foreach (var keyboardAccelerator in keyboardAccelerators)
				platformView.KeyboardAccelerators.Add(keyboardAccelerator);
			
		}

		// Single key (A, Delete, F2, Spacebar, Esc, Multimedia Key) accelerators and multi-key
		// accelerators (Ctrl+Shift+M) are supported.
		// Gamepad virtual keys are not supported.
		public static IList<KeyboardAccelerator>? ToPlatform(this IAccelerator accelerator)
		{
			if (accelerator is null)
				return null;

			List<KeyboardAccelerator> result = new List<KeyboardAccelerator>();
			
			var key = accelerator.Key;
			var modifiers = accelerator.Modifiers;

			if (modifiers is not null)
			{
				var modifiersCount = modifiers.Count();
				bool hasModifierMask = modifiersCount > 0;

				if (hasModifierMask)
				{
					if (modifiersCount == 1)
						result.Add(CreateSingleKeyAccelerator(modifiers!.ElementAt(0), key));
					else
						result.Add(CreateMultiKeyAccelerator(modifiers!, key));
				}
			}
			else
				result.Add(CreateSingleKeyAccelerator(string.Empty, key));

			return result;
		}

		internal static VirtualKeyModifiers ToVirtualKeyModifiers(this string modifierMask)
		{
			switch (modifierMask.ToLowerInvariant())
			{
				case "ctrl":
					return VirtualKeyModifiers.Control;
				case "alt":
					return VirtualKeyModifiers.Menu;
				case "shift":
					return VirtualKeyModifiers.Shift;
				case "win":
					return VirtualKeyModifiers.Windows;
				default:
					return VirtualKeyModifiers.None;
			}
		}

		internal static VirtualKey ToVirtualKey(this string key)
		{
			if (Enum.TryParse<VirtualKey>(key, true, out var virtualKey))
				return virtualKey;

			return VirtualKey.None;
		}
		
		internal static KeyboardAccelerator CreateSingleKeyAccelerator(string modifierMask, string key)
		{
			var keyboardAccelerator = new KeyboardAccelerator();

			if (!string.IsNullOrEmpty(modifierMask))
				keyboardAccelerator.Modifiers = modifierMask.ToVirtualKeyModifiers();

			keyboardAccelerator.Key = key.ToVirtualKey();

			return keyboardAccelerator;
		}

		internal static KeyboardAccelerator CreateMultiKeyAccelerator(IEnumerable<string> modifiers, string key)
		{
			var keyboardAccelerator = new KeyboardAccelerator();

			for (int i = 0; i < modifiers.Count(); i++)
			{
				var modifierMask = modifiers.ElementAt(i).ToLowerInvariant();
				keyboardAccelerator.Modifiers |= modifierMask.ToVirtualKeyModifiers();
			}
				
			keyboardAccelerator.Key = key.ToVirtualKey();

			return keyboardAccelerator;
		}
	}
}
