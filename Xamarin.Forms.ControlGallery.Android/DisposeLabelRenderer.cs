using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android
{
#pragma warning disable 618
	public class DisposeLabelRenderer :
#if !LEGACY_RENDERERS
		Platform.Android.FastRenderers.LabelRenderer
#else
		LabelRenderer
#endif
#pragma warning restore 618
	{
		protected override void Dispose (bool disposing)
		{

			if (disposing) {
				((DisposeLabel) Element).SendRendererDisposed ();
			}
			base.Dispose (disposing);
		}
	}
}