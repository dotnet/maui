namespace Xamarin.Forms
{
	internal sealed class CustomKeyboard : Keyboard
	{
		internal CustomKeyboard(KeyboardFlags flags)
		{
			Flags = flags;
		}

		internal KeyboardFlags Flags { get; private set; }
	}
}