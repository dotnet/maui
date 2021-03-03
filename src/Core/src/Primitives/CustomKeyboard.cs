using System.ComponentModel;

namespace Microsoft.Maui
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