using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android
{
#pragma warning disable 618
	public class DisposePageRenderer : PageRenderer
#pragma warning restore 618
	{
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				((DisposePage) Element).SendRendererDisposed ();
			}
			base.Dispose (disposing);

		}
	}
}