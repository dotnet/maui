using Android.Content.Res;
using Android.Text;
using Android.Widget;

namespace Microsoft.Maui
{
	public static class EntryExtensions
	{
		static readonly int[][] ColorStates = { 
			new[] { global::Android.Resource.Attribute.StateEnabled }, 
			new[] { -global::Android.Resource.Attribute.StateEnabled } 
		};

		public static void UpdateText(this EditText editText, IEntry entry)
		{
			editText.Text = entry.Text;
		}

		public static void UpdateTextColor(this EditText editText, IEntry entry, ColorStateList? defaultColor)
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

		public static void UpdateIsPassword(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		internal static void SetInputType(this EditText editText, IEntry entry)
		{
			editText.InputType = InputTypes.ClassText;
			editText.InputType |= InputTypes.TextFlagMultiLine;

			if (entry.IsPassword && ((editText.InputType & InputTypes.ClassText) == InputTypes.ClassText))
				editText.InputType |= InputTypes.TextVariationPassword;

			if (entry.IsPassword && ((editText.InputType & InputTypes.ClassNumber) == InputTypes.ClassNumber))
				editText.InputType |= InputTypes.NumberVariationPassword;

			if (!entry.IsTextPredictionEnabled && ((editText.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions))
				editText.InputType |= InputTypes.TextFlagNoSuggestions;
		}

		public static void UpdateIsTextPredictionEnabled(this EditText editText, IEntry entry)
		{
			editText.SetInputType(entry);
		}

		public static void UpdatePlaceholder(this EditText editText, IEntry entry)
		{
			if (editText.Hint == entry.Placeholder)
				return;

			editText.Hint = entry.Placeholder;
		}
	}
}
