#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{

	/// <summary>
	/// Represents a shortcut key for a <see cref="MenuItem"/>.
	/// </summary>
	[TypeConverter(typeof(AcceleratorTypeConverter))]
	public class KeyboardAccelerator : IKeyboardAccelerator
	{
		const string Separator = "+";
		readonly string _text;
		readonly List<string> _modifiers;

		internal KeyboardAccelerator(string text, IEnumerable<string> modifiers, string key)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));

			_text = text;
			Key = key;
			_modifiers = new List<string>(modifiers);
		}

		/// <summary>
		/// Gets the modifiers for the keyboard accelerator.
		/// </summary>
		public IEnumerable<string> Modifiers => _modifiers;

		IReadOnlyList<string> IKeyboardAccelerator.Modifiers => _modifiers;

		/// <summary>
		/// Gets the key for the keyboard accelerator.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// Returns a new <see cref="KeyboardAccelerator"/> from the text.
		/// </summary>
		/// <param name="text"></param>
		/// <returns><see cref="KeyboardAccelerator"/></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static KeyboardAccelerator FromString(string text)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));

			var str = text;
			var modifiers = new List<string>();
			var key = string.Empty;

			var acceleratorParts = text.Split(new[] { Separator }, StringSplitOptions.None);

			if (acceleratorParts.Length > 1)
			{
				for (int i = 0; i < acceleratorParts.Length; i++)
				{
					var modifierMask = acceleratorParts[i];
					var modiferMaskLower = modifierMask.ToLowerInvariant();
					switch (modiferMaskLower)
					{
						case "ctrl":
						case "cmd":
						case "alt":
						case "shift":
						case "fn":
						case "win":
							modifiers.Add(modiferMaskLower);
#if NETSTANDARD2_0
							text = text.Replace(modifierMask, string.Empty);
#else
							text = text.Replace(modifierMask, string.Empty, StringComparison.Ordinal);
#endif
							break;
					}
				}
			}

			if (!string.Equals(text, Separator, StringComparison.Ordinal))
			{
#if NETSTANDARD2_0
				text = text.Replace(Separator, string.Empty);
#else
				text = text.Replace(Separator, string.Empty, StringComparison.Ordinal);
#endif
			}

			key = text;

			return new KeyboardAccelerator(str, modifiers, key);
		}

		/// <summary>
		/// Returns a text representation of the <see cref="KeyboardAccelerator"/>.
		/// </summary>
		/// <returns><see cref="String"/></returns>
		public override string ToString()
		{
			return _text;
		}

		/// <summary>
		/// Compares `this` accelerator to `obj`.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns><see cref="Boolean"/> Returns `true` if `obj` is of type <see cref="KeyboardAccelerator"/> and is equal to it. Otherwise, returns false.</returns>
		public override bool Equals(object obj)
		{
			return obj is KeyboardAccelerator && Equals((KeyboardAccelerator)obj);
		}

		bool Equals(KeyboardAccelerator other)
		{
			return other.ToString() == ToString();
		}

		/// <summary>
		/// Returns the hash code for the lowercase string that represents the shortcut key.
		/// </summary>
		/// <returns><see cref="Int32"/></returns>
		public override int GetHashCode()
		{
#if NETSTANDARD2_0
			return _text.GetHashCode();
#else
			return _text.GetHashCode(StringComparison.Ordinal);
#endif
		}

		public static implicit operator KeyboardAccelerator(string accelerator)
		{
			return FromString(accelerator);
		}
	}
}
