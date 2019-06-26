using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Material.Tizen;
using Tizen.NET.MaterialComponents;
using TForms = Xamarin.Forms.Platform.Tizen.Forms;

[assembly: ExportRenderer(typeof(CheckBox), typeof(MaterialCheckBoxRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialCheckBoxRenderer : CheckBoxRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MCheckBox(TForms.NativeParent));
			}
			base.OnElementChanged(e);
		}
	}
}
