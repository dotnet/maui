using Android.Content.Res;
using Android.Widget;

namespace System.Maui.Platform
{
	/// <summary>
	/// Handles color state management for a TextView's TextColor and HintTextColor properties
	/// </summary>
	internal class TextColorSwitcher
	{
		static readonly int[][] s_colorStates = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

		readonly ColorStateList _defaultTextColors;
		Color _currentTextColor;

		readonly ColorStateList _defaultHintTextColors;
		Color _currentHintTextColor = Color.Default;

		public TextView Control { get; }

		public TextColorSwitcher(TextView control)
		{
			Control = control;
			_defaultTextColors = control.TextColors;
			_defaultHintTextColors = control.HintTextColors;
		}

		public void UpdateTextColor(Color color)
		{
			if (color == _currentTextColor)
				return;

			_currentTextColor = color;

			if (color.IsDefault)
			{
				Control.SetTextColor(_defaultTextColors);
			}
			else
			{
				var acolor = color.ToNative().ToArgb();
				Control.SetTextColor(new ColorStateList(s_colorStates, new[] { acolor, acolor }));
			}
		}

		public void UpdateHintTextColor(Color color)
		{
			if (color == _currentHintTextColor)
				return;

			_currentTextColor = color;

			if (color.IsDefault)
			{
				Control.SetHintTextColor(_defaultHintTextColors);
			}
			else
			{
				var acolor = color.ToNative().ToArgb();
				Control.SetHintTextColor(new ColorStateList(s_colorStates, new[] { acolor, acolor }));
			}
		}
	}
}