using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		UITextField GetNativeEntry(EntryHandler entryHandler) =>
			(UITextField)entryHandler.View;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		Color GetNativeTextColor(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextColor.ToColor();

		bool GetNativeIsPassword(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SecureTextEntry;

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Placeholder;

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).AutocorrectionType == UITextAutocorrectionType.Yes;

		bool GetNativeIsReadOnly(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).UserInteractionEnabled;

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.DecimalPad;

		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.EmailAddress;

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.PhonePad;

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.Url;

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);

			return nativeEntry.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEntry.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEntry.SpellCheckingType == UITextSpellCheckingType.Yes;
		}

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);

			return nativeEntry.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEntry.AutocorrectionType == UITextAutocorrectionType.Yes;
		}
	}
}