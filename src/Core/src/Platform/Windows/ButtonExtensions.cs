#nullable enable
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateStrokeColor(this Button platformButton, IButtonStroke buttonStroke)
		{
			var brush = buttonStroke.StrokeColor?.ToPlatform();

			if (brush is null)
				platformButton.Resources.RemoveKeys(StrokeColorResourceKeys);
			else
				platformButton.Resources.SetValueForAllKey(StrokeColorResourceKeys, brush);

			platformButton.RefreshThemeResources();
		}

		static readonly string[] StrokeColorResourceKeys =
		{
			"ButtonBorderBrush",
			"ButtonBorderBrushPointerOver",
			"ButtonBorderBrushPressed",
			"ButtonBorderBrushDisabled",
		};

		public static void UpdateStrokeThickness(this Button platformButton, IButtonStroke buttonStroke)
		{
			var thickness = buttonStroke.StrokeThickness;

			if (thickness >= 0)
				platformButton.Resources.SetValueForAllKey(StrokeThicknessResourceKeys, WinUIHelpers.CreateThickness(buttonStroke.StrokeThickness));
			else
				platformButton.Resources.RemoveKeys(StrokeThicknessResourceKeys);

			platformButton.RefreshThemeResources();
		}

		static readonly string[] StrokeThicknessResourceKeys =
		{
			"ButtonBorderThemeThickness",
		};

		public static void UpdateCornerRadius(this Button platformButton, IButtonStroke buttonStroke)
		{
			var radius = buttonStroke.CornerRadius;

			if (radius >= 0)
				platformButton.Resources.SetValueForAllKey(CornerRadiusResourceKeys, WinUIHelpers.CreateCornerRadius(radius));
			else
				platformButton.Resources.RemoveKeys(CornerRadiusResourceKeys);

			platformButton.RefreshThemeResources();
		}

		static readonly string[] CornerRadiusResourceKeys =
		{
			"ControlCornerRadius",
		};

		public static void UpdateText(this Button platformButton, IText text)
		{
			platformButton.UpdateText(text.Text);
		}

		public static void UpdateText(this Button platformButton, string text)
		{
			if (platformButton.GetContent<TextBlock>() is not TextBlock textBlock)
				return;

			textBlock.Text = text;

			textBlock.Visibility = string.IsNullOrEmpty(text)
				? UI.Xaml.Visibility.Collapsed
				: UI.Xaml.Visibility.Visible;
		}

		public static void UpdateBackground(this Button platformButton, IButton button)
		{
			var brush = button.Background?.ToPlatform();

			if (brush is null)
				platformButton.Resources.RemoveKeys(BackgroundResourceKeys);
			else
				platformButton.Resources.SetValueForAllKey(BackgroundResourceKeys, brush);

			platformButton.RefreshThemeResources();
		}

		static readonly string[] BackgroundResourceKeys =
		{
			"ButtonBackground",
			"ButtonBackgroundPointerOver",
			"ButtonBackgroundPressed",
			"ButtonBackgroundDisabled",
		};

		public static void UpdateTextColor(this ButtonBase platformButton, ITextStyle button)
		{
			UpdateTextColor(platformButton, button.TextColor);
		}

		public static void UpdateTextColor(this ButtonBase platformButton, Color textColor)
		{
			platformButton.UpdateTextColor(textColor, TextColorResourceKeys);
		}

		internal static void UpdateTextColor(this ButtonBase platformButton, Color textColor, string[] resourceKeys)
		{
			var brush = textColor?.ToPlatform();

			if (brush is null)
			{
				// Windows.Foundation.UniversalApiContract < 5
				platformButton.Resources.RemoveKeys(resourceKeys);
				// Windows.Foundation.UniversalApiContract >= 5
				platformButton.ClearValue(Button.ForegroundProperty);
			}
			else
			{
				// Windows.Foundation.UniversalApiContract < 5
				platformButton.Resources.SetValueForAllKey(resourceKeys, brush);
				// Windows.Foundation.UniversalApiContract >= 5
				platformButton.Foreground = brush;
			}

			platformButton.RefreshThemeResources();
		}

		static readonly string[] TextColorResourceKeys =
		{
			"ButtonForeground",
			"ButtonForegroundPointerOver",
			"ButtonForegroundPressed",
			"ButtonForegroundDisabled",
		};

		public static void UpdatePadding(this Button platformButton, IPadding padding) =>
			platformButton.UpdatePadding(padding, platformButton.GetResource<UI.Xaml.Thickness>("ButtonPadding"));

		public static void UpdateCharacterSpacing(this ButtonBase platformButton, ITextStyle button)
		{
			var characterSpacing = button.CharacterSpacing.ToEm();

			platformButton.CharacterSpacing = characterSpacing;

			if (platformButton.GetContent<TextBlock>() is TextBlock textBlock)
				textBlock.CharacterSpacing = characterSpacing;
		}

		public static void UpdateImageSource(this Button platformButton, WImageSource? nativeImageSource)
		{
			if (platformButton.GetContent<WImage>() is WImage nativeImage)
			{
				// Stretch to fill
				nativeImage.Stretch = UI.Xaml.Media.Stretch.Uniform;

				// If we're a CanvasImageSource (font image source), we need to explicitly set the image height
				// to the desired size of the font, otherwise it will be stretched to the available space
				if (nativeImageSource is CanvasImageSource canvas)
				{
					var size = canvas.GetImageSourceSize(platformButton);
					nativeImage.Width = size.Width;
					nativeImage.Height = size.Height;
					nativeImage.MaxHeight = double.PositiveInfinity;
				}

				// Ensure that we only scale images down and never up
				if (nativeImageSource is BitmapImage bitmapImage)
				{
					// This will fire after `nativeImageSource.Source` is set
					bitmapImage.ImageOpened += OnImageOpened;
					void OnImageOpened(object sender, RoutedEventArgs e)
					{
						bitmapImage.ImageOpened -= OnImageOpened;

						var actualImageSource = sender as BitmapImage;
						if (actualImageSource is not null)
						{
							nativeImage.MaxHeight = nativeImageSource.GetImageSourceSize(platformButton).Height;
						}
					}
				}

				nativeImage.Source = nativeImageSource;
				nativeImage.Visibility = nativeImageSource == null
					? UI.Xaml.Visibility.Collapsed
					: UI.Xaml.Visibility.Visible;
			}
		}

		public static T? GetContent<T>(this ButtonBase platformButton)
			where T : FrameworkElement
		{
			if (platformButton.Content is null)
				return null;

			if (platformButton.Content is T t)
				return t;

			if (platformButton.Content is Panel panel)
			{
				foreach (var child in panel.Children)
				{
					if (child is T c)
						return c;
				}
			}

			return null;
		}
	}
}