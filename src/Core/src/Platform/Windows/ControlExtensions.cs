using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Platform
{
	public static class ControlExtensions
	{
		public static void UpdateFont(this Control platformButton, ITextStyle textStyle, IFontManager fontManager) =>
			platformButton.UpdateFont(textStyle.Font, fontManager);

		public static void UpdateFont(this Control platformControl, Font font, IFontManager fontManager)
		{
			platformControl.FontSize = fontManager.GetFontSize(font);
			platformControl.FontFamily = fontManager.GetFontFamily(font);
			platformControl.FontStyle = font.ToFontStyle();
			platformControl.FontWeight = font.ToFontWeight();
			platformControl.IsTextScaleFactorEnabled = font.AutoScalingEnabled;
		}

		public static void UpdateIsEnabled(this Control platformControl, bool isEnabled) =>
			platformControl.IsEnabled = isEnabled;

		public static void UpdateBackground(this Control platformControl, IView view, WBrush? defaultBrush = null) =>
			SetBackground(platformControl, view.Background, defaultBrush);

		public static void UpdateBackground(this Control platformControl, Paint? paint, WBrush? defaultBrush = null) =>
			SetBackground(platformControl, paint, defaultBrush);

		public static void UpdateBackground(this Border platformControl, Paint? paint, WBrush? defaultBrush = null) =>
			SetBackground(platformControl, paint, defaultBrush);

		public static void UpdateBackground(this Panel platformControl, Paint? paint, WBrush? defaultBrush = null) =>
			SetBackground(platformControl, paint, defaultBrush);

		public static Task UpdateBackgroundImageSourceAsync(this Panel platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider) =>
			SetBackgroundImageSourceAsync(platformView, imageSource, provider);

		public static Task UpdateBackgroundImageSourceAsync(this Control platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider) =>
			SetBackgroundImageSourceAsync(platformView, imageSource, provider);

		public static void UpdateForegroundColor(this Control platformControl, Color color, WBrush? defaultBrush = null) =>
			platformControl.Foreground = color?.ToPlatform() ?? defaultBrush ?? platformControl.Foreground;

		public static void UpdatePadding(this Control platformControl, IPadding padding, UI.Xaml.Thickness? defaultThickness = null) =>
			platformControl.UpdatePadding(padding.Padding, defaultThickness);

		public static void UpdatePadding(this Control platformControl, Thickness padding, UI.Xaml.Thickness? defaultThickness = null)
		{
			platformControl.Padding = padding.IsNaN
				? defaultThickness ?? new UI.Xaml.Thickness()
				: padding.ToPlatform();
		}

		public static void UpdateTextColor(this Control nativeControl, ITextStyle text) =>
			nativeControl.UpdateProperty(Control.ForegroundProperty, text.TextColor);

		public static void UpdateCharacterSpacing(this Control nativeControl, ITextStyle text) =>
			nativeControl.CharacterSpacing = text.CharacterSpacing.ToEm();

		/// <summary>
		/// Set the "background" of a framework element. This will set the Background property if
		/// the element is a Control, Panel or Border. It will set the Fill property if the element
		/// is a Shape.
		/// </summary>
		/// <param name="platformControl">The Control, Border, Panel or Shape to apply a background.</param>
		/// <param name="paint">The background paint to apply.</param>
		/// <param name="defaultBrush">An optional fallback brush to apply if the paint is empty.</param>
		internal static void SetBackground(this FrameworkElement platformControl, Paint? paint, WBrush? defaultBrush = null)
		{
			var brush = paint.IsNullOrEmpty()
				? defaultBrush
				: paint?.ToPlatform();

			platformControl.SetBackground(brush);
		}

		/// <summary>
		/// Set the "background" of a framework element. This will set the Background property if
		/// the element is a Control, Panel or Border. It will set the Fill property if the element
		/// is a Shape.
		/// </summary>
		/// <param name="platformControl">The Control, Border, Panel or Shape to apply a background.</param>
		/// <param name="brush">The background brush to apply.</param>
		internal static void SetBackground(this FrameworkElement platformControl, WBrush? brush)
		{
			if (platformControl is Control control)
			{
				control.UpdateProperty(Control.BackgroundProperty, brush);
			}
			else if (platformControl is Border border)
			{
				border.UpdateProperty(Border.BackgroundProperty, brush);
			}
			else if (platformControl is Panel panel)
			{
				panel.UpdateProperty(Panel.BackgroundProperty, brush);
			}
			else if (platformControl is Shape shape)
			{
				shape.UpdateProperty(Shape.FillProperty, brush);
			}
		}

		/// <summary>
		/// Set the "background" of a framework element to be the image source.
		/// </summary>
		/// <param name="platformView">The Control, Border, Panel or Shape to apply a background.</param>
		/// <param name="imageSource">The image source to apply.</param>
		/// <param name="provider">The image source service provider to use.</param>
		internal static async Task SetBackgroundImageSourceAsync(this FrameworkElement platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
		{
			if (platformView == null || provider == null)
				return;

			if (imageSource == null)
			{
				platformView.SetBackground(null);
				return;
			}

			if (provider != null && imageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var nativeBackgroundImageSource = await service.GetImageSourceAsync(imageSource);

				var background = new ImageBrush { ImageSource = nativeBackgroundImageSource?.Value };

				platformView.SetBackground(background);
			}
		}
	}
}