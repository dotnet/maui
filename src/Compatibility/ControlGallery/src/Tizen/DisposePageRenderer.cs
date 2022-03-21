using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
	public class DisposePageRenderer : PageRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposePage)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}
}