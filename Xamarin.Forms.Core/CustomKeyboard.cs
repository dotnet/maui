using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	public sealed class CustomKeyboard : Keyboard
	{
		internal CustomKeyboard(KeyboardFlags flags)
		{
			Flags = flags;
		}


		[EditorBrowsable(EditorBrowsableState.Never)]
		public KeyboardFlags Flags { get; private set; }
	}
}