using Xamarin.Forms;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class KeyboardExtensions
	{
		/// <summary>
		/// Creates an instance of ElmSharp.Keyboard reflecting the provided Xamarin.Forms.Keyboard instance
		/// </summary>
		/// <returns>Keyboard type corresponding to the provided Xamarin.Forms.Keyboard</returns>
		/// <param name="keyboard">The Xamarin.Forms.Keyboard class instance to be converted to ElmSharp.Keyboard.</param>
		public static Native.Keyboard ToNative(this Keyboard keyboard)
		{
			if (keyboard == Keyboard.Numeric)
			{
				return Native.Keyboard.Numeric;
			}
			else if (keyboard == Keyboard.Telephone)
			{
				return Native.Keyboard.PhoneNumber;
			}
			else if (keyboard == Keyboard.Email)
			{
				return Native.Keyboard.Email;
			}
			else if (keyboard == Keyboard.Url)
			{
				return Native.Keyboard.Url;
			}
			else
			{
				return Native.Keyboard.Normal;
			}
		}
	}
}