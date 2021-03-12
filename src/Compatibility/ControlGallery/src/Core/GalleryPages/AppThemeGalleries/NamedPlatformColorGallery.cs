using System.Collections.Generic;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.AppThemeGalleries
{
	public class NamedPlatformColorGallery : ContentPage
	{
		readonly string[] _colors;
		readonly StackLayout _stackLayout = new StackLayout
		{
			Padding = new Thickness(10)
		};

		public NamedPlatformColorGallery()
		{
			Content = new ScrollView
			{
				Content = _stackLayout
			};

			_colors = GetPlatFormColorNames();

			BuildColors();
		}

		void BuildColors()
		{
			if (_colors == null || _colors.Length == 0)
				return;

			_stackLayout.Children.Clear();

			for (var i = 0; i < _colors.Length; i++)
			{
				var color = Device.GetNamedColor(_colors[i]);

				var box = new Frame
				{
					BackgroundColor = color,
					Content = new StackLayout
					{
						Children =
						{
							new Label { Text = _colors[i] },
							new Label { Text = color.ToHex() }
						}
					},
					HasShadow = false,
					Margin = 10,
					HeightRequest = 60
				};

				_stackLayout.Children.Add(box);
			}
		}

		string[] GetPlatFormColorNames()
		{
			List<string> resultColors = new List<string>();

			if (Device.RuntimePlatform == Device.macOS || Device.RuntimePlatform == Device.iOS)
			{
				resultColors.AddRange(new[] {
					NamedPlatformColor.SystemBlue,
					NamedPlatformColor.SystemGreen,
					NamedPlatformColor.SystemIndigo,
					NamedPlatformColor.SystemOrange,
					NamedPlatformColor.SystemPink,
					NamedPlatformColor.SystemPurple,
					NamedPlatformColor.SystemRed,
					NamedPlatformColor.SystemTeal,
					NamedPlatformColor.SystemYellow,
					NamedPlatformColor.SystemGray,
					NamedPlatformColor.Label,
					NamedPlatformColor.SecondaryLabel,
					NamedPlatformColor.TertiaryLabel,
					NamedPlatformColor.QuaternaryLabel,
					NamedPlatformColor.PlaceholderText,
					NamedPlatformColor.Separator,
					NamedPlatformColor.Link
				});
			}

			if (Device.RuntimePlatform == Device.iOS)
			{
				resultColors.AddRange(new[] {
					NamedPlatformColor.SystemGray2,
					NamedPlatformColor.SystemGray3,
					NamedPlatformColor.SystemGray4,
					NamedPlatformColor.SystemGray5,
					NamedPlatformColor.SystemGray6,
					NamedPlatformColor.OpaqueSeparator
				});
			}

			if (Device.RuntimePlatform == Device.macOS)
			{
				resultColors.AddRange(new[] {
					NamedPlatformColor.AlternateSelectedControlTextColor,
					NamedPlatformColor.ControlAccent,
					NamedPlatformColor.ControlBackgroundColor,
					NamedPlatformColor.ControlColor,
					NamedPlatformColor.ControlTextColor,
					NamedPlatformColor.DisabledControlTextColor,
					NamedPlatformColor.FindHighlightColor,
					NamedPlatformColor.GridColor,
					NamedPlatformColor.HeaderTextColor,
					NamedPlatformColor.HighlightColor,
					NamedPlatformColor.KeyboardFocusIndicatorColor,
					NamedPlatformColor.LabelColor,
					NamedPlatformColor.LinkColor,
					NamedPlatformColor.PlaceholderTextColor,
					NamedPlatformColor.QuaternaryLabelColor,
					NamedPlatformColor.SecondaryLabelColor,
					NamedPlatformColor.SelectedContentBackgroundColor,
					NamedPlatformColor.SelectedControlColor,
					NamedPlatformColor.SelectedControlTextColor,
					NamedPlatformColor.SelectedMenuItemTextColor,
					NamedPlatformColor.SelectedTextBackgroundColor,
					NamedPlatformColor.SelectedTextColor,
					NamedPlatformColor.SeparatorColor,
					NamedPlatformColor.ShadowColor,
					NamedPlatformColor.TertiaryLabelColor,
					NamedPlatformColor.TextBackgroundColor,
					NamedPlatformColor.TextColor,
					NamedPlatformColor.UnderPageBackgroundColor,
					NamedPlatformColor.UnemphasizedSelectedContentBackgroundColor,
					NamedPlatformColor.UnemphasizedSelectedTextBackgroundColor,
					NamedPlatformColor.UnemphasizedSelectedTextColor,
					NamedPlatformColor.WindowBackgroundColor,
					NamedPlatformColor.WindowFrameTextColor
				});
			}

			if (Device.RuntimePlatform == Device.Android)
			{
				resultColors.AddRange(new[] {
					NamedPlatformColor.BackgroundDark,
					NamedPlatformColor.BackgroundLight,
					NamedPlatformColor.Black,
					NamedPlatformColor.DarkerGray,
					NamedPlatformColor.HoloBlueBright,
					NamedPlatformColor.HoloBlueDark,
					NamedPlatformColor.HoloBlueLight,
					NamedPlatformColor.HoloGreenDark,
					NamedPlatformColor.HoloGreenLight,
					NamedPlatformColor.HoloOrangeDark,
					NamedPlatformColor.HoloOrangeLight,
					NamedPlatformColor.HoloPurple,
					NamedPlatformColor.HoloRedDark,
					NamedPlatformColor.HoloRedLight,
					NamedPlatformColor.TabIndicatorText,
					NamedPlatformColor.Transparent,
					NamedPlatformColor.White,
					NamedPlatformColor.WidgetEditTextDark
				});
			}

			if (Device.RuntimePlatform == Device.UWP)
			{
				resultColors.AddRange(new[] {
					NamedPlatformColor.SystemAltLowColor,
					NamedPlatformColor.SystemAltMediumColor,
					NamedPlatformColor.SystemAltMediumHighColor,
					NamedPlatformColor.SystemAltMediumLowColor,
					NamedPlatformColor.SystemBaseHighColor,
					NamedPlatformColor.SystemBaseLowColor,
					NamedPlatformColor.SystemBaseMediumColor,
					NamedPlatformColor.SystemBaseMediumHighColor,
					NamedPlatformColor.SystemBaseMediumLowColor,
					NamedPlatformColor.SystemChromeAltLowColor,
					NamedPlatformColor.SystemChromeBlackHighColor,
					NamedPlatformColor.SystemChromeBlackLowColor,
					NamedPlatformColor.SystemChromeBlackMediumLowColor,
					NamedPlatformColor.SystemChromeBlackMediumColor,
					NamedPlatformColor.SystemChromeDisabledHighColor,
					NamedPlatformColor.SystemChromeDisabledLowColor,
					NamedPlatformColor.SystemChromeHighColor,
					NamedPlatformColor.SystemChromeLowColor,
					NamedPlatformColor.SystemChromeMediumColor,
					NamedPlatformColor.SystemChromeMediumLowColor,
					NamedPlatformColor.SystemChromeWhiteColor,
					NamedPlatformColor.SystemListLowColor,
					NamedPlatformColor.SystemListMediumColor,
					NamedPlatformColor.SystemAltHighColor
				});
			}

			return resultColors.ToArray();
		}
	}
}