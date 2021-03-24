using System.Collections.Generic;
using Android.Content.Res;
using Android.Text;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class EditTextExtensions
	{
		static readonly int[][] ColorStates =
		{
			new[] { Android.Resource.Attribute.StateEnabled },
			new[] { -Android.Resource.Attribute.StateEnabled }
		};

		public static void UpdateText(this AppCompatEditText editText, IEntry entry)
		{
			editText.UpdateText(entry.Text);

			// TODO ezhart The renderer sets the text to selected and shows the keyboard if the EditText is focused
		}

		public static void UpdateText(this AppCompatEditText editText, IEditor editor)
		{
			editText.UpdateText(editor.Text);

			editText.SetSelection(editText.Text?.Length ?? 0);
		}

		public static void UpdateTextColor(this AppCompatEditText editText, IEntry entry, ColorStateList? defaultColor)
		{
			var textColor = entry.TextColor;
			if (textColor.IsDefault)
			{
				editText.SetTextColor(defaultColor);
			}
			else
			{
				var androidColor = textColor.ToNative();

				if (!editText.TextColors.IsOneColor(ColorStates, androidColor))
				{
					var acolor = androidColor.ToArgb();
					editText.SetTextColor(new ColorStateList(ColorStates, new[] { acolor, acolor }));
				}
			}
		}

		public static void UpdateIsPassword(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateHorizontalTextAlignment(this AppCompatEditText editText, IEntry entry)
		{
			editText.UpdateHorizontalAlignment(entry.HorizontalTextAlignment, editText.Context != null && editText.Context.HasRtlSupport());
		}

		public static void UpdateIsTextPredictionEnabled(this AppCompatEditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdateIsTextPredictionEnabled(this AppCompatEditText editText, IEditor editor)
		{
			if (editor.IsTextPredictionEnabled)
				editText.InputType &= ~InputTypes.TextFlagNoSuggestions;
			else
				editText.InputType |= InputTypes.TextFlagNoSuggestions;
		}

		public static void UpdateMaxLength(this AppCompatEditText editText, IEntry entry) =>
			UpdateMaxLength(editText, entry.MaxLength);

		public static void UpdateMaxLength(this AppCompatEditText editText, IEditor editor) =>
			UpdateMaxLength(editText, editor.MaxLength);

		public static void UpdateMaxLength(this AppCompatEditText editText, int maxLength)
		{
			var currentFilters = new List<IInputFilter>(editText.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(maxLength));

			editText.SetFilters(currentFilters.ToArray());

			editText.Text = TrimToMaxLength(editText.Text, maxLength);
		}

		public static void UpdatePlaceholder(this AppCompatEditText editText, IEntry entry)
		{
			if (editText.Hint == entry.Placeholder)
				return;

			editText.Hint = entry.Placeholder;
		}

		public static void UpdateIsReadOnly(this AppCompatEditText editText, IEntry entry)
		{
			bool isEditable = !entry.IsReadOnly;

			editText.SetInputType(entry);

			editText.FocusableInTouchMode = isEditable;
			editText.Focusable = isEditable;
		}

		public static void UpdateFont(this AppCompatEditText editText, IEntry entry, IFontManager fontManager) =>
			editText.UpdateFont(entry.Font, fontManager);

		public static void UpdateReturnType(this AppCompatEditText editText, IEntry entry)
		{
			editText.ImeOptions = entry.ReturnType.ToNative();
		}

		public static void UpdateFont(this AppCompatEditText editText, IEditor editor, IFontManager fontManager)
		{
			var font = editor.Font;

			var tf = fontManager.GetTypeface(font);
			editText.Typeface = tf;

			var sp = fontManager.GetScaledPixel(font);
			editText.SetTextSize(Android.Util.ComplexUnitType.Sp, sp);
		}

		internal static void SetInputType(this AppCompatEditText editText, IEntry entry)
		{
			editText.InputType = InputTypes.ClassText;
			editText.InputType |= InputTypes.TextFlagMultiLine;

			if (entry.IsPassword && ((editText.InputType & InputTypes.ClassText) == InputTypes.ClassText))
				editText.InputType |= InputTypes.TextVariationPassword;

			if (entry.IsPassword && ((editText.InputType & InputTypes.ClassNumber) == InputTypes.ClassNumber))
				editText.InputType |= InputTypes.NumberVariationPassword;

			if (!entry.IsTextPredictionEnabled && ((editText.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions))
				editText.InputType |= InputTypes.TextFlagNoSuggestions;

			if (entry.IsReadOnly)
				editText.InputType = InputTypes.Null;
		}

		internal static string? TrimToMaxLength(string? currentText, int maxLength) =>
			currentText?.Length > maxLength
				? currentText.Substring(0, maxLength)
				: currentText;
	}
}