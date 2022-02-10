using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="Type[@FullName='Microsoft.Maui.Controls.Accelerator']/Docs" />
	[System.ComponentModel.TypeConverter(typeof(AcceleratorTypeConverter))]
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Modifiers']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<string> Modifiers { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Keys']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<string> Keys { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='FromString']/Docs" />
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
					var modiferMaskLower = modifierMask.ToLower();
					switch (modiferMaskLower)
					{
						case "ctrl":
						case "cmd":
						case "alt":
						case "shift":
						case "fn":
						case "win":
							modifiers.Add(modiferMaskLower);
							text = text.Replace(modifierMask, "");
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString()
		{
			return _text;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='Equals']/Docs" />
		public override bool Equals(object obj)
		{
			return obj is Accelerator && Equals((Accelerator)obj);
		}

		bool Equals(Accelerator other)
		{
			return other.ToString() == ToString();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Accelerator.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public static implicit operator Accelerator(string accelerator)
		{
			return FromString(accelerator);
		}
	}
}
