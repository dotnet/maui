using AutoCapital = Tizen.NUI.InputMethod.AutoCapitalType;
using TKeyboard = Tizen.UIExtensions.Common.Keyboard;
using PlatformView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static partial class KeyboardExtensions
	{
		public static TKeyboard ToPlatform(this Keyboard keyboard)
		{
			if (keyboard == Keyboard.Numeric)
			{
				return TKeyboard.Numeric;
			}
			else if (keyboard == Keyboard.Telephone)
			{
				return TKeyboard.PhoneNumber;
			}
			else if (keyboard == Keyboard.Email)
			{
				return TKeyboard.Email;
			}
			else if (keyboard == Keyboard.Url)
			{
				return TKeyboard.Url;
			}
			else
			{
				return TKeyboard.Normal;
			}
		}

		public static AutoCapital ToAutoCapital(this KeyboardFlags keyboardFlags)
		{
			if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				return AutoCapital.Sentence;
			}
			else if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				return AutoCapital.Word;
			}
			else if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				return AutoCapital.Allcharacter;
			}
			else
			{
				return AutoCapital.None;
			}
		}

		internal static bool HideKeyboard(this PlatformView view) => SetKeyInputFocus(view, false);

		internal static bool ShowKeyboard(this PlatformView view) => SetKeyInputFocus(view, true);

		internal static bool IsSoftKeyboardShowing(this PlatformView view)
		{
			return view.KeyInputFocus;
		}

		internal static bool SetKeyInputFocus(PlatformView view, bool isShow)
		{
			view.KeyInputFocus = isShow;

			return view.KeyInputFocus;
		}
	}
}