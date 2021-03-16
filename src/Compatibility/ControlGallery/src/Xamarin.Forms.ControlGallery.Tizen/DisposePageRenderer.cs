using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Tizen;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
namespace Xamarin.Forms.ControlGallery.Tizen
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