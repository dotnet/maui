using Android.Content.Res;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	/// <summary>
	/// Handles color state management for the TextColor property 
	/// for Button, Picker, TimePicker, and DatePicker
	/// </summary>
	internal class TextColorSwitcher
	{
		static readonly int[][] s_colorStates = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		readonly ColorStateList _defaultTextColors;
		Color _currentTextColor;

		public TextColorSwitcher(ColorStateList textColors)
		{
			_defaultTextColors = textColors;
		}

		public void UpdateTextColor(TextView control, Color color)
		{
			if (color == _currentTextColor)
				return;

			_currentTextColor = color;

			if (color.IsDefault)
				control.SetTextColor(_defaultTextColors);
			else
			{
				// Set the new enabled state color, preserving the default disabled state color
				int defaultDisabledColor = _defaultTextColors.GetColorForState(s_colorStates[1], color.ToAndroid());
				control.SetTextColor(new ColorStateList(s_colorStates, new[] { color.ToAndroid().ToArgb(), defaultDisabledColor }));
			}
		}

	}
}