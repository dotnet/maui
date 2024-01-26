#nullable enable
using System.ComponentModel;
using Microsoft.Maui.Primitives;

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
#if ANDROID
		static Keyboard? s_date;

		static Keyboard? s_time;

		static Keyboard? s_password;
#endif


		internal Keyboard()
		{
		}

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Plain']/Docs/*" />
		public static Keyboard Plain => s_plain ??= new CustomKeyboard(KeyboardFlags.None);

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Chat']/Docs/*" />
		public static Keyboard Chat => s_chat ??= new ChatKeyboard();

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Default']/Docs/*" />
		public static Keyboard Default => s_def ??= new Keyboard();

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Email']/Docs/*" />
		public static Keyboard Email => s_email ??= new EmailKeyboard();

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Numeric']/Docs/*" />
		public static Keyboard Numeric => s_numeric ??= new NumericKeyboard();

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Telephone']/Docs/*" />
		public static Keyboard Telephone => s_telephone ??= new TelephoneKeyboard();

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public static Keyboard Text => s_text ??= new TextKeyboard();

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Url']/Docs/*" />
		public static Keyboard Url => s_url ??= new UrlKeyboard();
# if __ANDROID__
		public static Keyboard Date => s_date ??= new DateKeyboard();

		public static Keyboard Password => s_password ??= new PasswordKeyboard();

		public static Keyboard Time => s_time ??= new TimeKeyboard();
#endif

		/// <include file="../../docs/Microsoft.Maui/Keyboard.xml" path="//Member[@MemberName='Create']/Docs/*" />
		public static Keyboard Create(KeyboardFlags flags)
		{
			return new CustomKeyboard(flags);
		}
	}
}