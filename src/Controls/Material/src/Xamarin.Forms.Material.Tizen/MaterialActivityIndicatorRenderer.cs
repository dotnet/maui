using Tizen.NET.MaterialComponents;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialActivityIndicatorRenderer : ActivityIndicatorRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MActivityIndicator(Forms.NativeParent));
			}

			base.OnElementChanged(e);
		}
	}
}
