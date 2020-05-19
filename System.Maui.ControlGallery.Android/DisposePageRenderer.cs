using System.Maui.Controls;
using System.Maui.Platform.Android;

namespace System.Maui.ControlGallery.Android
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