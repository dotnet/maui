using System.Maui;
using System.Maui.Platform.Tizen;
using System.Maui.Material.Tizen;
using Tizen.NET.MaterialComponents;

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialActivityIndicatorRenderer : ActivityIndicatorRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MActivityIndicator(System.Maui.Maui.NativeParent));
			}

			base.OnElementChanged(e);
		}
	}
}
