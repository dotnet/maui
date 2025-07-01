using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class TimePickerExtensions
	{
		public static void UpdateTime(this TimePicker nativeTimePicker, ITimePicker timePicker)
		{
			nativeTimePicker.Time = timePicker.Time;

			if (timePicker.Format?.Contains('H', StringComparison.Ordinal) == true)
			{
				nativeTimePicker.ClockIdentifier = "24HourClock";
			}
			else
			{
				nativeTimePicker.ClockIdentifier = "12HourClock";
			}
		}

		public static void UpdateCharacterSpacing(this TimePicker platformTimePicker, ITimePicker timePicker)
		{
			platformTimePicker.CharacterSpacing = timePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this TimePicker platformTimePicker, ITimePicker timePicker, IFontManager fontManager) =>
			platformTimePicker.UpdateFont(timePicker.Font, fontManager);

		public static void UpdateTextColor(this TimePicker platformTimePicker, ITimePicker timePicker)
		{
			Color textColor = timePicker.TextColor;

			UI.Xaml.Media.Brush? platformBrush = textColor?.ToPlatform();

			if (platformBrush == null)
			{
				platformTimePicker.Resources.RemoveKeys(TextColorResourceKeys);
				platformTimePicker.ClearValue(TimePicker.ForegroundProperty);
			}
			else
			{
				platformTimePicker.Resources.SetValueForAllKey(TextColorResourceKeys, platformBrush);
				platformTimePicker.Foreground = platformBrush;
			}

			platformTimePicker.RefreshThemeResources();
		}

		// Make it public in .NET 10.
		internal static void UpdateTextAlignment(this TimePicker platformTimePicker, ITimePicker timePicker)
		{
			var flowDirection = timePicker.FlowDirection;
			var textAlignment = flowDirection == FlowDirection.RightToLeft
				? UI.Xaml.TextAlignment.Right
				: UI.Xaml.TextAlignment.Left;


			SetTextAlignment(platformTimePicker, "HourTextBlock", textAlignment);
			SetTextAlignment(platformTimePicker, "MinuteTextBlock", textAlignment);
			SetTextAlignment(platformTimePicker, "PeriodTextBlock", textAlignment);
		}

		static void SetTextAlignment(TimePicker platformTimePicker, string elementName, Microsoft.UI.Xaml.TextAlignment textAlignment)
		{
			var textBlock = platformTimePicker.GetDescendantByName<TextBlock>(elementName);
			if (textBlock is not null)
			{
				textBlock.TextAlignment = textAlignment;
			}
		}

		// ResourceKeys controlling the foreground color of the TimePicker.
		// https://docs.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.timepicker?view=windows-app-sdk-1.1
		static readonly string[] TextColorResourceKeys =
		{
			"TimePickerButtonForeground",
			"TimePickerButtonForegroundPointerOver",
			"TimePickerButtonForegroundPressed",
			"TimePickerButtonForegroundDisabled"
		};

		// TODO NET8 add to public API
		internal static void UpdateBackground(this TimePicker platformTimePicker, ITimePicker timePicker)
		{
			var brush = timePicker?.Background?.ToPlatform();

			if (brush is null)
			{
				platformTimePicker.Resources.RemoveKeys(BackgroundColorResourceKeys);
				platformTimePicker.ClearValue(TimePicker.BackgroundProperty);
			}
			else
			{
				platformTimePicker.Resources.SetValueForAllKey(BackgroundColorResourceKeys, brush);
				platformTimePicker.Background = brush;
			}

			platformTimePicker.RefreshThemeResources();
		}

		static readonly string[] BackgroundColorResourceKeys =
		{
			"TimePickerButtonBackground",
			"TimePickerButtonBackgroundPointerOver",
			"TimePickerButtonBackgroundPressed",
			"TimePickerButtonBackgroundDisabled",
			"TimePickerButtonBackgroundFocused",
		};
	}
}
