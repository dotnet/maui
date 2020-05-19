using System.Maui;
using System.Maui.Platform.Tizen;
using System.Maui.Material.Tizen;
using Tizen.NET.MaterialComponents;

[assembly: ExportRenderer(typeof(ProgressBar), typeof(MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace System.Maui.Material.Tizen
{
	public class MaterialProgressBarRenderer : ProgressBarRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MProgressIndicator(System.Maui.Maui.NativeParent));
			}
			base.OnElementChanged(e);
		}
	}
}
