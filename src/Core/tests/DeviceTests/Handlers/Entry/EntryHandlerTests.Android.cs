using Android.Text;
using Android.Text.Method;
using Android.Widget;
using Microsoft.Maui.Handlers;
using AColor = global::Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		EditText GetNativeEntry(EntryHandler entryHandler) =>
			(EditText)entryHandler.View;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		Color GetNativeTextColor(EntryHandler entryHandler)
		{
			int currentTextColorInt = GetNativeEntry(entryHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		bool GetNativeIsPassword(EntryHandler entryHandler)
		{
			var inputType = GetNativeEntry(entryHandler).InputType;
			return inputType.HasFlag(InputTypes.TextVariationPassword) || inputType.HasFlag(InputTypes.NumberVariationPassword);
		}

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).InputType.HasFlag(InputTypes.TextFlagNoSuggestions);


		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Hint;

		bool GetNativeIsReadOnly(EntryHandler entryHandler) 
		{
			var editText = GetNativeEntry(entryHandler);

			return !editText.Focusable && !editText.FocusableInTouchMode;
		}

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return editText.KeyListener is NumberKeyListener 
				&& (inputTypes.HasFlag(InputTypes.NumberFlagDecimal) && inputTypes.HasFlag(InputTypes.ClassNumber) && inputTypes.HasFlag(InputTypes.NumberFlagSigned));
		}

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return (inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagNoSuggestions));
		}


		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return (inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationEmailAddress));
		}

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassPhone);
		}

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationUri);
		}

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences);
		}
	}
}