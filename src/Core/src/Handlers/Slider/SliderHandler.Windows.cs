#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, Slider>
	{
		static Brush? DefaultForegroundColor;		
		static Brush? DefaultBackgroundColor;

		protected override Slider CreateNativeView()
		{
			var slider = new Slider
			{
				IsThumbToolTipEnabled = false
			};

			return slider;
		}

		protected override void SetupDefaults(Slider nativeView)
		{
			DefaultForegroundColor = nativeView.Foreground;
			DefaultBackgroundColor = nativeView.Background;
		}

		public static void MapMinimum(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMinimum(slider);
		}

		public static void MapMaximum(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMaximum(slider);
		}

		public static void MapValue(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateValue(slider);
		}

		public static void MapMinimumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMinimumTrackColor(slider, DefaultForegroundColor);
		}

		public static void MapMaximumTrackColor(SliderHandler handler, ISlider slider)
		{
			handler.NativeView?.UpdateMaximumTrackColor(slider, DefaultBackgroundColor);
		}

		[MissingMapper]
		public static void MapThumbColor(SliderHandler handler, ISlider slider) { }
	}
}