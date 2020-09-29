using Tizen.NET.MaterialComponents;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(ProgressBar), typeof(MaterialProgressBarRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
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
