#nullable enable
using System.ComponentModel;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="Type[@FullName='Microsoft.Maui.Keyboard']/Docs/*" />
	[TypeConverter(typeof(Converters.KeyboardTypeConverter))]
	public class Keyboard
	{
		static Keyboard? s_plain;

		static Keyboard? s_def;

		static Keyboard? s_email;

		static Keyboard? s_text;

		static Keyboard? s_url;

		static Keyboard? s_numeric;

		static Keyboard? s_telephone;

		static Keyboard? s_chat;

		internal Keyboard()
		{
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Plain']/Docs/*" />
		public static Keyboard Plain
		{
			get { return s_plain ??= new CustomKeyboard(KeyboardFlags.None); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Chat']/Docs/*" />
		public static Keyboard Chat
		{
			get { return s_chat ??= new ChatKeyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Default']/Docs/*" />
		public static Keyboard Default
		{
			get { return s_def ??= new Keyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Email']/Docs/*" />
		public static Keyboard Email
		{
			get { return s_email ??= new EmailKeyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Numeric']/Docs/*" />
		public static Keyboard Numeric
		{
			get { return s_numeric ??= new NumericKeyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Telephone']/Docs/*" />
		public static Keyboard Telephone
		{
			get { return s_telephone ??= new TelephoneKeyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public static Keyboard Text
		{
			get { return s_text ??= new TextKeyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Url']/Docs/*" />
		public static Keyboard Url
		{
			get { return s_url ??= new UrlKeyboard(); }
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Create']/Docs/*" />
		public static Keyboard Create(KeyboardFlags flags)
		{
			return new CustomKeyboard(flags);
		}
	}
}