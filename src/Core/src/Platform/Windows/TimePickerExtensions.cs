using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.ViewManagement.Core;

namespace Microsoft.Maui.Platform;

public static class TimePickerExtensions
{
	public static void UpdateTime(this TimePicker nativeTimePicker, ITimePicker timePicker)
	{
		nativeTimePicker.Time = timePicker.Time ?? TimeSpan.Zero;

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

		if (platformTimePicker.IsLoaded)
		{
			UpdateCharacterSpacingInTimePicker(platformTimePicker);
		}
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

	// ResourceKeys controlling the foreground color of the TimePicker.
	// https://docs.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.timepicker?view=windows-app-sdk-1.1
	static readonly string[] TextColorResourceKeys =
	{
		"TimePickerButtonForeground",
		"TimePickerButtonForegroundPointerOver",
		"TimePickerButtonForegroundPressed",
		"TimePickerButtonForegroundDisabled"
	};

	public static void UpdateBackground(this TimePicker platformTimePicker, ITimePicker timePicker)
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

	static void UpdateCharacterSpacingInTimePicker(this TimePicker platformTimePicker)
	{
		string[] partNames = { "HourTextBlock", "MinuteTextBlock", "PeriodTextBlock" };
		foreach (var partName in partNames)
		{
			SetCharacterSpacingToBlocks(platformTimePicker, partName);
		}
	}

	static void SetCharacterSpacingToBlocks(TimePicker platformTimePicker, string partName)
	{
		var textBlock = platformTimePicker.GetDescendantByName<TextBlock>(partName);
		if (textBlock is not null)
		{
			textBlock.CharacterSpacing = platformTimePicker.CharacterSpacing;
		}
	}

	static readonly string[] BackgroundColorResourceKeys =
	{
		"TimePickerButtonBackground",
		"TimePickerButtonBackgroundPointerOver",
		"TimePickerButtonBackgroundPressed",
		"TimePickerButtonBackgroundDisabled",
		"TimePickerButtonBackgroundFocused",
	};

	internal static void UpdateIsOpen(this TimePicker platformTimePicker, ITimePicker timePicker)
	{
		if (!platformTimePicker.IsLoaded)
		{
			RoutedEventHandler? onLoaded = null;
			onLoaded = (s, e) =>
			{
				platformTimePicker.Loaded -= onLoaded;
				UpdateIsOpen(platformTimePicker, timePicker);
			};
			platformTimePicker.Loaded += onLoaded;
			return;
		}

		if (timePicker.IsOpen)
		{
			platformTimePicker.Focus(FocusState.Programmatic);

			// Create automation peer for the TimePicker to invoke a click
			var peer = FrameworkElementAutomationPeer.CreatePeerForElement(platformTimePicker) ?? new TimePickerAutomationPeer(platformTimePicker);

			// Look for button inside and invoke it
			var children = peer.GetChildren();

			if (children is null)
				return;

			foreach (var child in children)
			{
				if (child.GetClassName().Contains("Button", StringComparison.OrdinalIgnoreCase) &&
					child.GetPattern(PatternInterface.Invoke) is IInvokeProvider childInvoke)
				{
					childInvoke.Invoke();
					return;
				}
			}
		}
		else
		{
			// Lost the WinUI TimePicker focus.
			var parent = VisualTreeHelper.GetParent(platformTimePicker) as UIElement;
			parent?.Focus(FocusState.Programmatic);
		}
	}
}
