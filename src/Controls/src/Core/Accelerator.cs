#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="Type[@FullName='Microsoft.Maui.Controls.Accelerator']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(AcceleratorTypeConverter))]
	public class Accelerator : IAccelerator
	{
		const string Separator = "+";
		string _text;

		internal Accelerator(string text, List<string> modifiers, string key)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));
			_text = text;
			Key = key;
			Modifiers = modifiers;
		}

		/// <summary>
		/// Gets the modifiers for the accelerator.
		/// </summary>
		public IEnumerable<string> Modifiers { get; }

		IReadOnlyList<string> IAccelerator.Modifiers => Modifiers?.ToList();

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Keys']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use Key instead.")]
		public IEnumerable<string> Keys 
		{
			get => Key is null ? null : new[] { Key };
		}

		/// <summary>
		/// Gets the key for the accelerator.
		/// </summary>
		public string Key { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='FromString']/Docs/*" />
		public static Accelerator FromString(string text)
		{
			var modifiers = new List<string>();
			var key = "";

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
							text = text.Replace(modifierMask, "");
#else
							text = text.Replace(modifierMask, "", StringComparison.Ordinal);
#endif
							break;
					}
				}
			}

			if (!string.Equals(text, Separator, StringComparison.Ordinal))
			{
#if NETSTANDARD2_0
				text = text.Replace(Separator, "");
#else
				text = text.Replace(Separator, "", StringComparison.Ordinal);
#endif
			}

			key = text;

			return new Accelerator(text, modifiers, key);
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
}
