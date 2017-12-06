namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Keyboard layout type on entry control.
	/// </summary>
	public enum Keyboard
	{
		/// <summary>
		/// Disable Keyboard
		/// </summary>
		None = -1,

		/// <summary>
		/// Keyboard layout type default.
		/// </summary>
		Normal,

		/// <summary>
		/// Keyboard layout type number.
		/// </summary>
		Number,

		/// <summary>
		/// Keyboard layout type email.
		/// </summary>
		Email,

		/// <summary>
		/// Keyboard layout type url.
		/// </summary>
		Url,

		/// <summary>
		/// Keyboard layout type phone.
		/// </summary>
		PhoneNumber,

		/// <summary>
		/// Keyboard layout type ip.
		/// </summary>
		Ip,

		/// <summary>
		/// Keyboard layout type month.
		/// </summary>
		Month,

		/// <summary>
		/// Keyboard layout type number.
		/// </summary>
		NumberOnly,

		/// <summary>
		/// Keyboard layout type error type. Do not use it directly!
		/// </summary>
		Invalid,

		/// <summary>
		/// Keyboard layout type hexadecimal.
		/// </summary>
		Hex,

		/// <summary>
		/// Keyboard layout type terminal type, esc, alt, ctrl, etc.
		/// </summary>
		Terminal,

		/// <summary>
		/// Keyboard layout type password.
		/// </summary>
		Password,

		/// <summary>
		/// Keyboard layout type date and time.
		/// </summary>
		DateTime,

		/// <summary>
		/// Keyboard layout type emoticons.
		/// </summary>
		Emoticon,

		/// <summary>
		/// Keyboard layout type numeric.
		/// </summary>
		Numeric = Emoticon + 2017,
	}
}

