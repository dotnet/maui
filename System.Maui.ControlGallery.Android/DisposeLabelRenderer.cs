using System.Maui.Controls;
using System.Maui.Platform.Android;

namespace System.Maui.ControlGallery.Android
{
#pragma warning disable 618
	public class DisposeLabelRenderer :
#if TEST_EXPERIMENTAL_RENDERERS
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