using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
#pragma warning disable CS0618 // Type or member is obsolete
	public class DisposeLabelRenderer : LabelRenderer
#pragma warning disable CS0618 // Type or member is obsolete
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