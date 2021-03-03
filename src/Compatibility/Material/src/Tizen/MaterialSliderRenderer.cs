using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;

[assembly: ExportRenderer(typeof(Slider), typeof(MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
{
	public class MaterialSliderRenderer : SliderRenderer
	{
		protected override ElmSharp.Slider CreateNativeControl()
		{
			return new MSlider(Forms.NativeParent);
		}

		protected override void UpdateMinimumTrackColor()
		{
			var color = Element.MinimumTrackColor.IsDefault ? MColors.Current.PrimaryColor : Element.MinimumTrackColor.ToNative();
			Control.SetPartColor(Parts.Slider.Bar, color);
			Control.SetPartColor(Parts.Slider.BarPressed, color);
		}

		protected override void UpdateMaximumTrackColor()
		{
			var color = Element.MaximumTrackColor.IsDefault ? MColors.Current.PrimaryColor.WithAlpha(0.5) : Element.MaximumTrackColor.ToNative();
			Control.SetPartColor(Parts.Widget.Background, color);
		}

		protected override void UpdateThumbColor()
		{
			var color = Element.ThumbColor.IsDefault ? MColors.Current.PrimaryColor : Element.ThumbColor.ToNative();
			Control.SetPartColor(Parts.Slider.Handler, color);
			Control.SetPartColor(Parts.Slider.HandlerPressed, color);
			Control.SetPartColor(Parts.Slider.Handler2, color.WithAlpha(0.32));
		}

	}
}
