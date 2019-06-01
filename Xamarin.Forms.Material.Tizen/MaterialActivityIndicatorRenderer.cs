using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Material.Tizen;
using Tizen.NET.MaterialComponents;
using TForms = Xamarin.Forms.Platform.Tizen.Forms;

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialActivityIndicatorRenderer : ActivityIndicatorRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MActivityIndicator(TForms.NativeParent));
			}

			base.OnElementChanged(e);
		}
	}
}
