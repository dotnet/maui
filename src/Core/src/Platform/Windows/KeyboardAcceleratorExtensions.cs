using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;

namespace Microsoft.Maui.Platform
{
	public static class KeyboardAcceleratorExtensions
	{
		public static void UpdateAccelerator(this MenuFlyoutItemBase platformView, IMenuFlyoutItem menuFlyoutItem)
		{
			var keyboardAccelerators = menuFlyoutItem.Accelerator.ToPlatform();

			if (keyboardAccelerators is null)
				return;

			foreach (var keyboardAccelerator in keyboardAccelerators)
			{
				platformView.KeyboardAccelerators.Add(keyboardAccelerator);
			}
		}
		
		public static IList<KeyboardAccelerator>? ToPlatform(this IAccelerator accelerator)
		{
			if (accelerator is null)
				return null;

			List<KeyboardAccelerator> result = new List<KeyboardAccelerator>();

			var keys = accelerator.Keys;
			var modifiers = accelerator.Modifiers;

			bool hasModifierMask = modifiers.Any();

			if (hasModifierMask)
			{
				for (int i = 0; i < modifiers.Count(); i++)
				{
					var keyboardAccelerator = new KeyboardAccelerator();

					var modifierMask = modifiers.ElementAt(i).ToLowerInvariant();

					switch (modifierMask)
					{
						case "ctrl":
							keyboardAccelerator.Modifiers = VirtualKeyModifiers.Control;
							break;
						case "alt":
							keyboardAccelerator.Modifiers = VirtualKeyModifiers.Menu;
							break;
						case "shift":
							keyboardAccelerator.Modifiers = VirtualKeyModifiers.Shift;
							break;
						default:
							keyboardAccelerator.Modifiers = VirtualKeyModifiers.None;
							break;
					}

					string key = keys.ElementAt(i);

					if (Enum.TryParse<VirtualKey>(key, true, out var virtualKey))
						keyboardAccelerator.Key = virtualKey;

					result.Add(keyboardAccelerator);
				}
			}

			return result;
		}
	}
}
