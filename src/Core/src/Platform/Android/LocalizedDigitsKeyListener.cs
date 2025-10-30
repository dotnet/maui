﻿using System.Collections.Generic;
using System.Globalization;
using Android.Text;
using Android.Text.Method;
using Java.Lang;
using Java.Text;

namespace Microsoft.Maui.Platform
{
	public class LocalizedDigitsKeyListener : NumberKeyListener
	{
		readonly char _decimalSeparator;

		// I'm not aware of a situation/locale where this would need to be something different, 
		// but we'll make it easy to localize the sign in the future just in case
		const char SignCharacter = '-';

		static Dictionary<char, LocalizedDigitsKeyListener>? UnsignedCache;
		static Dictionary<char, LocalizedDigitsKeyListener>? SignedCache;

		public static NumberKeyListener Create(InputTypes inputTypes)
		{
			if ((inputTypes & InputTypes.NumberFlagDecimal) == 0)
			{
				// If decimal isn't allowed, we can just use the Android version
#pragma warning disable CS0618, CA1416, CA1422 // Deprecated in API 26: https://developer.android.com/reference/android/text/method/DigitsKeyListener#getInstance(boolean,%20boolean)
				return DigitsKeyListener.GetInstance(inputTypes.HasFlag(InputTypes.NumberFlagSigned), false);
#pragma warning restore CS0618, CA1416, CA1422
			}

			// Figure out what the decimal separator is for the current locale
			var symbol = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			var decimalSeparator = string.IsNullOrEmpty(symbol) ? '.' : symbol[0];

			if (decimalSeparator == '.')
			{
				// If it's '.', then we can just use the default Android version
#pragma warning disable CS0618, CA1416, CA1422 // Deprecated in API 26: https://developer.android.com/reference/android/text/method/DigitsKeyListener#getInstance(boolean,%20boolean)
				return DigitsKeyListener.GetInstance(inputTypes.HasFlag(InputTypes.NumberFlagSigned), true);
#pragma warning restore CS0618, CA1416, CA1422
			}

			// If decimals are enabled and the locale's decimal separator is not '.'
			// (which is hard-coded in the Android DigitKeyListener), then use 
			// our custom one with a configurable decimal separator
			return GetInstance(inputTypes, decimalSeparator);
		}

		public static LocalizedDigitsKeyListener GetInstance(InputTypes inputTypes, char decimalSeparator)
		{
			if (SignedCache == null)
				SignedCache = new Dictionary<char, LocalizedDigitsKeyListener>();

			if ((inputTypes & InputTypes.NumberFlagSigned) != 0)
			{
				return GetInstance(inputTypes, decimalSeparator, ref SignedCache);
			}

			if (UnsignedCache == null)
				UnsignedCache = new Dictionary<char, LocalizedDigitsKeyListener>();

			return GetInstance(inputTypes, decimalSeparator, ref UnsignedCache);
		}

		static LocalizedDigitsKeyListener GetInstance(InputTypes inputTypes, char decimalSeparator, ref Dictionary<char, LocalizedDigitsKeyListener> cache)
		{
			if (cache == null)
			{
				cache = new Dictionary<char, LocalizedDigitsKeyListener>(1);
			}

			if (!cache.ContainsKey(decimalSeparator))
			{
				cache.Add(decimalSeparator, new LocalizedDigitsKeyListener(inputTypes, decimalSeparator));
			}

			return cache[decimalSeparator];
		}

		protected LocalizedDigitsKeyListener(InputTypes inputTypes, char decimalSeparator)

		{
			_decimalSeparator = decimalSeparator;
			InputType = inputTypes;
		}

		public override InputTypes InputType { get; }

		char[]? _acceptedChars;

		protected override char[] GetAcceptedChars()
		{
			if ((InputType & InputTypes.NumberFlagSigned) == 0)
			{
				return _acceptedChars ??= new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', _decimalSeparator };
			}

			return _acceptedChars ??= new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', SignCharacter, _decimalSeparator };
		}

		static bool IsSignChar(char c)
		{
			return c == SignCharacter;
		}

		bool IsDecimalPointChar(char c)
		{
			return c == _decimalSeparator;
		}

#pragma warning disable CS8602 // Dereference of a possibly null reference.
		public override ICharSequence? FilterFormatted(ICharSequence? source, int start, int end, ISpanned? dest, int dstart, int dend)
		{
			// Borrowed heavily from the Android source
			ICharSequence? filterFormatted = base.FilterFormatted(source, start, end, dest, dstart, dend);

			if (filterFormatted != null)
			{
				source = filterFormatted;
				start = 0;
				end = filterFormatted.Length();
			}

			int sign = -1;
			int dec = -1;

			int dlen = dest.Length();


			// Find out if the existing text has a sign or decimal point characters.
			for (var i = 0; i < dstart; i++)
			{
				char c = dest.CharAt(i);
				if (IsSignChar(c))
				{
					sign = i;
				}
				else if (IsDecimalPointChar(c))
				{
					dec = i;
				}
			}

			for (int i = dend; i < dlen; i++)
			{
				char c = dest.CharAt(i);
				if (IsSignChar(c))
				{
					return new String(""); // Nothing can be inserted in front of a sign character.
				}

				if (IsDecimalPointChar(c))
				{
					dec = i;
				}
			}

			// If it does, we must strip them out from the source.
			// In addition, a sign character must be the very first character,
			// and nothing can be inserted before an existing sign character.
			// Go in reverse order so the offsets are stable.
			SpannableStringBuilder? stripped = null;
			for (int i = end - 1; i >= start; i--)
			{
				char c = source.CharAt(i);
				var strip = false;

				if (IsSignChar(c))
				{
					if (i != start || dstart != 0)
					{
						strip = true;
					}
					else if (sign >= 0)
					{
						strip = true;
					}
					else
					{
						sign = i;
					}
				}
				else if (IsDecimalPointChar(c))
				{
					if (dec >= 0)
					{
						strip = true;
					}
					else
					{
						dec = i;
					}
				}

				if (strip)
				{
					if (end == start + 1)
					{
						return new String(""); // Only one character, and it was stripped.
					}
					if (stripped == null)
					{
						stripped = new SpannableStringBuilder(source, start, end);
					}
					stripped.Delete(i - start, i + 1 - start);
				}
			}

			return stripped ?? filterFormatted;
		}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}
}