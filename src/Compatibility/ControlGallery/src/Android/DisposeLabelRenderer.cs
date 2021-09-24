using Android.Content;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
#pragma warning disable 618
	public class DisposeLabelRenderer :
#if !LEGACY_RENDERERS
		Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.LabelRenderer
#else
		LabelRenderer
#endif
#pragma warning restore 618
	{
		public DisposeLabelRenderer(Context context) : base(context)
		{
		}

		protected override void Dispose(bool disposing)
		{

			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}
}