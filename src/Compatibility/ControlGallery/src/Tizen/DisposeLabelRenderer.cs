using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
namespace  Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}
}