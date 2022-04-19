using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
#pragma warning disable CS0618 // Type or member is obsolete
	public class DisposePageRenderer : PageRenderer
#pragma warning disable CS0618 // Type or member is obsolete
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