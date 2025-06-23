using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Platform
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this RadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.IsChecked = radioButton.IsChecked;
		}

		private static readonly string[] _backgroundColorKeys =
		{
			"RadioButtonBackground",
			"RadioButtonBackgroundPointerOver",
			"RadioButtonBackgroundPressed",
			"RadioButtonBackgroundDisabled"
		};

		public static void UpdateBackground(this RadioButton platformRadioButton, IRadioButton button)
		{
			if (button.Background is SolidPaint solidPaint)
			{
				UpdateColors(platformRadioButton.Resources, _backgroundColorKeys, solidPaint.ToPlatform());

				platformRadioButton.RefreshThemeResources();
			}
		}

		private static readonly string[] _foregroundColorKeys =
		{
			"RadioButtonForeground",
			"RadioButtonForegroundPointerOver",
			"RadioButtonForegroundPressed",
			"RadioButtonForegroundDisabled"
		};

		public static void UpdateTextColor(this RadioButton platformRadioButton, ITextStyle button)
		{
			UpdateColors(platformRadioButton.Resources, _foregroundColorKeys, button.TextColor?.ToPlatform());

			platformRadioButton.RefreshThemeResources();
		}

		public static void UpdateContent(this RadioButton platformRadioButton, IRadioButton radioButton)
		{
			_ = radioButton.Handler?.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (radioButton.PresentedContent is IView view)
				platformRadioButton.Content = view.ToPlatform(radioButton.Handler.MauiContext);
			else
				platformRadioButton.Content = $"{radioButton.Content}";
		}

		private static readonly string[] _borderColorKeys =
		{
			"RadioButtonBorderBrush",
			"RadioButtonBorderBrushPointerOver",
			"RadioButtonBorderBrushPressed",
			"RadioButtonBorderBrushDisabled"
		};

		public static void UpdateStrokeColor(this RadioButton platformRadioButton, IRadioButton radioButton)
		{
			UpdateColors(platformRadioButton.Resources, _borderColorKeys, radioButton.StrokeColor?.ToPlatform());

			platformRadioButton.RefreshThemeResources();
		}

		static void UpdateColors(ResourceDictionary resource, string[] keys, WBrush? brush)
		{
			if (brush is null)
				resource.RemoveKeys(keys);
			else
				resource.SetValueForAllKey(keys, brush);
		}

		public static void UpdateStrokeThickness(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			// Ensure stroke thickness is non-negative; a negative value causes an exception, so it defaults to zero if unset or invalid.
			nativeRadioButton.BorderThickness = radioButton.StrokeThickness < 0 ? WinUIHelpers.CreateThickness(0) : WinUIHelpers.CreateThickness(radioButton.StrokeThickness);
		}

		public static void UpdateCornerRadius(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.CornerRadius = WinUIHelpers.CreateCornerRadius(radioButton.CornerRadius);
		}
	}
}