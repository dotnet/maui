using Tizen.NET.MaterialComponents;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(Slider), typeof(MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialSliderRenderer : SliderRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MSlider(Forms.NativeParent));
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
