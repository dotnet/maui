using System.ComponentModel;

namespace Microsoft.Maui
{
	/// <summary>
	/// Default keyboard and base class for specialized keyboards, such as those for telephone numbers, email, and URLs.
	/// </summary>
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

		static Keyboard? s_date;

		static Keyboard? s_time;

		static Keyboard? s_password;


		internal Keyboard()
		{
		}

		/// <summary>
		/// Returns a new keyboard with no special flags.
		/// </summary>
		public static Keyboard Plain => s_plain ??= new CustomKeyboard(KeyboardFlags.None);

		/// <summary>
		/// Gets a keyboard optimized for chat input.
		/// </summary>
		public static Keyboard Chat => s_chat ??= new ChatKeyboard();

		/// <summary>
		/// Gets the default keyboard.
		/// </summary>
		public static Keyboard Default => s_def ??= new Keyboard();

		/// <summary>
		/// Gets a keyboard optimized for email input.
		/// </summary>
		public static Keyboard Email => s_email ??= new EmailKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for numeric input.
		/// </summary>
		public static Keyboard Numeric => s_numeric ??= new NumericKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for telephone number input.
		/// </summary>
		public static Keyboard Telephone => s_telephone ??= new TelephoneKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for text input.
		/// </summary>
		public static Keyboard Text => s_text ??= new TextKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for URL input.
		/// </summary>
		public static Keyboard Url => s_url ??= new UrlKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for date input.
		/// </summary>
		public static Keyboard Date => s_date ??= new DateKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for password input.
		/// </summary>
		public static Keyboard Password => s_password ??= new PasswordKeyboard();

		/// <summary>
		/// Gets a keyboard optimized for time input.
		/// </summary>
		public static Keyboard Time => s_time ??= new TimeKeyboard();


		/// <summary>
		/// Returns a new keyboard with the specified <see cref="KeyboardFlags" />.
		/// </summary>
		/// <param name="flags">The flags that control the keyboard's appearance and behavior.</param>
		/// <returns>A new <see cref="CustomKeyboard"/> instance with the specified <see cref="KeyboardFlags" />.</returns>
		public static Keyboard Create(KeyboardFlags flags)
		{
			return new CustomKeyboard(flags);
		}
	}
}