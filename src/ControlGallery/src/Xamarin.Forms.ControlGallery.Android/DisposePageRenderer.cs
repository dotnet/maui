using Android.Content;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android
{
	public class DisposePageRenderer : PageRenderer
	{
		public DisposePageRenderer(Context context) : base(context)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				((DisposePage) Element).SendRendererDisposed ();
			}
			base.Dispose (disposing);

		}
	}
}