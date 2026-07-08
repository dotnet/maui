using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;

[assembly: ExportRenderer(typeof(ActivityIndicator), typeof(MaterialActivityIndicatorRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
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
