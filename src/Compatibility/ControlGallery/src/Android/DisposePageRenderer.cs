using Android.Content;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class DisposePageRenderer : PageRenderer
	{
		public DisposePageRenderer(Context context) : base(context)
		{
		}

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