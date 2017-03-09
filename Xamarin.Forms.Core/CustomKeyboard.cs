using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class CustomKeyboard : Keyboard
	{
		internal CustomKeyboard(KeyboardFlags flags)
		{
			Flags = flags;
		}


		public KeyboardFlags Flags { get; private set; }
	}
}