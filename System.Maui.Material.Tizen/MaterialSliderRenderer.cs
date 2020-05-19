using System.Maui;
using System.Maui.Platform.Tizen;
using System.Maui.Material.Tizen;
using Tizen.NET.MaterialComponents;

[assembly: ExportRenderer(typeof(Slider), typeof(MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialSliderRenderer : SliderRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MSlider(System.Maui.Maui.NativeParent));
			}
			base.OnElementChanged(e);
		}

		protected override void UpdateThumbColor()
		{
			var color = Element.ThumbColor.IsDefault ? MColors.Current.PrimaryColor : Element.ThumbColor.ToNative();
			Control.SetPartColor(Parts.Slider.Handler, color);
			Control.SetPartColor(Parts.Slider.HandlerPressed, color);
		}
	}
}
