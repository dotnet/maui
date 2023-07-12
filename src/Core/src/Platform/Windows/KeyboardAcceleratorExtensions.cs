using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Group of helper extension methods related to KeyboardAccelerators.
	/// </summary>
	public static class KeyboardAcceleratorExtensions
	{
		/// <summary>
		/// Updates the collection of KeyboardAccelerators used by a MenuFlyoutItemBase.
		/// </summary>
		/// <param name="platformView">The platform view, of type <see cref="MenuFlyoutItemBase"/>.</param>
		/// <param name="menuFlyoutItem">The abstract menu flyout item, of type <see cref="IMenuFlyoutItem"/>, with all the necessary information.</param>
		public static void UpdateAccelerator(this MenuFlyoutItemBase platformView, IMenuFlyoutItem menuFlyoutItem)
		{
			var keyboardAccelerators = menuFlyoutItem.Accelerators?.ToPlatform();

			if (keyboardAccelerators is null)
				return;

			foreach (var keyboardAccelerator in keyboardAccelerators)
				platformView.KeyboardAccelerators.Add(keyboardAccelerator);
		}

		/// <summary>
		/// Converts a list of IAccelerator to a list of KeyboardAccelerator.
		/// A KeyboardAccelerator represents a keyboard shortcut (or accelerator) that lets a user perform an action using the keyboard instead
		/// of navigating the app UI (directly or through access keys). 
		/// </summary>
		/// <param name="accelerators">List of <see cref="IAccelerator"/></param>
		/// <returns>List of <see cref="KeyboardAccelerator"/></returns>
		public static IList<KeyboardAccelerator>? ToPlatform(this IReadOnlyList<IAccelerator> accelerators)
		{
			if (accelerators is null)
				return null;

			List<KeyboardAccelerator> result = new List<KeyboardAccelerator>();

			foreach (var accelerator in accelerators)
			{
				var keyboardAccelerators = accelerator.ToPlatform();

				if (keyboardAccelerators is not null)
					result.AddRange(keyboardAccelerators);
			}

			return result;
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

			var keyboardAccelerator = new KeyboardAccelerator();
			keyboardAccelerator.Key = key.ToVirtualKey();
			if (modifiers is not null)
			{
				foreach (var mod in modifiers)
				{
					keyboardAccelerator.Modifiers |= mod.ToVirtualKeyModifiers();
				}
			}
			result.Add(keyboardAccelerator);

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
			if (int.TryParse(key, out int numberKey))
			{
				if (numberKey >= 0 && numberKey <= 9)
				{
					key = $"Number{key}";
				}
			}

			if (Enum.TryParse<VirtualKey>(key.ToVirtualKeyString(), true, out var virtualKey))
				return virtualKey;

			return VirtualKey.None;
		}

		internal static string ToVirtualKeyString(this string key)
		{
			// VirtualKey is an enum. Each option can be accessed with the constant name or with an integer.
			// A common possible mistake using accelerators is to directly use digits like 3 to create an accelerator
			// like Ctrl+3, but we can access enum values using integers, which would give a different result
			// than desired. Here we try to avoid it.
			if (int.TryParse(key, out int numberKey))
			{
				if (numberKey >= 0 && numberKey <= 9)
				{
					key = $"Number{key}";
				}
			}

			return key;
		}
	}
}
