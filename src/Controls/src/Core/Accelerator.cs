#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="Type[@FullName='Microsoft.Maui.Controls.Accelerator']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(AcceleratorTypeConverter))]
	[ValueConverter(typeof(AcceleratorValueConverter))]
	[Obsolete("Use KeyboardAccelerator instead.")]
	public class Accelerator
	{
		const char Separator = '+';
		string _text;

		internal Accelerator(string text)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));
			_text = text;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Modifiers']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<string> Modifiers { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Keys']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<string> Keys { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='FromString']/Docs/*" />
		public static Accelerator FromString(string text)
		{
			var accelarat = new Accelerator(text);

			var acceleratorParts = text.Split(Separator);

			if (acceleratorParts.Length > 1)
			{
				var modifiers = new List<string>();
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
							text = text.Replace(modifierMask, "");
#else
							text = text.Replace(modifierMask, "", StringComparison.Ordinal);
#endif
							break;
					}
				}
				accelarat.Modifiers = modifiers;

			}

			if (text != Separator.ToString())
			{
				var keys = text.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
				accelarat.Keys = keys;
			}
			else
			{
				accelarat.Keys = new[] { text };
			}
			return accelarat;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString()
		{
			return _text;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Equals']/Docs/*" />
		public override bool Equals(object obj)
		{
			return obj is Accelerator && Equals((Accelerator)obj);
		}

		bool Equals(Accelerator other)
		{
			return other.ToString() == ToString();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode()
		{
#if NETSTANDARD2_0
			return _text.GetHashCode();
#else
			return _text.GetHashCode(StringComparison.Ordinal);
#endif
		}

		public static implicit operator Accelerator(string accelerator)
		{
			return FromString(accelerator);
		}
	}

#nullable enable
	internal sealed class AcceleratorValueConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> null;

		public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> value switch
			{
#pragma warning disable CS0618 // 'Accelerator' is obsolete: 'Use KeyboardAccelerator instead.'
				string accelerator => (Accelerator)accelerator,
#pragma warning restore CS0618
				_ => null,
			};
	}
}