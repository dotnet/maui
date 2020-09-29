using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(KeyboardTypeConverter))]
	public class Keyboard
	{
		static Keyboard s_plain;

		static Keyboard s_def;

		static Keyboard s_email;

		static Keyboard s_text;

		static Keyboard s_url;

		static Keyboard s_numeric;

		static Keyboard s_telephone;

		static Keyboard s_chat;

		internal Keyboard()
		{
		}

		public static Keyboard Plain
		{
			get { return s_plain ?? (s_plain = new CustomKeyboard(KeyboardFlags.None)); }
		}

		public static Keyboard Chat
		{
			get { return s_chat ?? (s_chat = new ChatKeyboard()); }
		}

		public static Keyboard Default
		{
			get { return s_def ?? (s_def = new Keyboard()); }
		}

		public static Keyboard Email
		{
			get { return s_email ?? (s_email = new EmailKeyboard()); }
		}

		public static Keyboard Numeric
		{
			get { return s_numeric ?? (s_numeric = new NumericKeyboard()); }
		}

		public static Keyboard Telephone
		{
			get { return s_telephone ?? (s_telephone = new TelephoneKeyboard()); }
		}

		public static Keyboard Text
		{
			get { return s_text ?? (s_text = new TextKeyboard()); }
		}

		public static Keyboard Url
		{
			get { return s_url ?? (s_url = new UrlKeyboard()); }
		}

		public static Keyboard Create(KeyboardFlags flags)
		{
			return new CustomKeyboard(flags);
		}
	}
}