using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;

[assembly: ExportRenderer(typeof(ProgressBar), typeof(MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
{
	public class MaterialProgressBarRenderer : ProgressBarRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MProgressIndicator(Forms.NativeParent));
			}
			base.OnElementChanged(e);
		}
	}
}
