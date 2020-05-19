using System.Maui;
using System.Maui.Controls;
using System.Maui.ControlGallery.Tizen;
using System.Maui.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
namespace System.Maui.ControlGallery.Tizen
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