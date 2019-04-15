using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Material.Tizen;
using Tizen.NET.MaterialComponents;
using TForms = Xamarin.Forms.Platform.Tizen.Forms;

[assembly: ExportRenderer(typeof(Slider), typeof(MaterialSliderRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialSliderRenderer : SliderRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MSlider(TForms.NativeParent));
			}
			base.OnElementChanged(e);
		}
	}
}
